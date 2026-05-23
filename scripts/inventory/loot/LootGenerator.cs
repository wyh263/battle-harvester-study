using Godot;
using System.Collections.Generic;
using BattleHarvesterStudy.Inventory.Search;
using BattleHarvesterStudy.Items;

namespace BattleHarvesterStudy.Inventory;

public static class LootGenerator
{
	public static List<ItemInstance> GenerateItems(LootTableDefinition table, RandomNumberGenerator? random = null, LootGenerationContext? context = null)
	{
		RandomNumberGenerator generator = random ?? new RandomNumberGenerator();
		if (random == null)
		{
			generator.Randomize();
		}

		List<ItemInstance> items = [];
		List<LootEntryDefinition> weightedEntries = [];
		foreach (LootEntryDefinition entry in table.Entries)
		{
			if (!entry.IsConfigured)
			{
				continue;
			}

			if (entry.GuaranteedDrop)
			{
				items.Add(CreateItemInstance(entry, generator));
			}

			if (entry.Weight > 0)
			{
				weightedEntries.Add(entry);
			}
		}

		int rollCount = table.GetRollCount(generator);
		if (table.UsesRarityBuckets)
		{
			for (int i = 0; i < rollCount; i++)
			{
				LootRarity selectedRarity = SelectRarityByWeight(table.RarityWeights, generator, context);
				ItemCategory selectedCategory = SelectCategoryByWeight(table.CategoryWeights, generator);
				LootBucketDefinition? selectedBucket = SelectBucket(table.Buckets, selectedRarity, selectedCategory, generator);
				if (selectedBucket == null)
				{
					selectedBucket = SelectAnyBucketForRarity(table.Buckets, selectedRarity, generator)
						?? SelectAnyBucket(table.Buckets, generator);
				}

				if (selectedBucket == null)
				{
					continue;
				}

				LootEntryDefinition? selectedEntry = SelectEntryFromBucket(selectedBucket, generator);
				if (selectedEntry == null)
				{
					continue;
				}

				items.Add(CreateItemInstance(selectedEntry, generator));
			}
		}
		else if (table.UsesCategoryPools)
		{
			List<LootCategoryPoolDefinition> weightedPools = [];
			foreach (LootCategoryPoolDefinition pool in table.CategoryPools)
			{
				if (pool.IsConfigured)
				{
					weightedPools.Add(pool);
				}
			}

			for (int i = 0; i < rollCount && weightedPools.Count > 0; i++)
			{
				LootCategoryPoolDefinition selectedPool = SelectPoolByWeight(weightedPools, generator, context);
				LootEntryDefinition? selectedEntry = SelectEntryFromPool(selectedPool, generator);
				if (selectedEntry == null)
				{
					continue;
				}

				items.Add(CreateItemInstance(selectedEntry, generator));
			}
		}
		else
		{
			for (int i = 0; i < rollCount && weightedEntries.Count > 0; i++)
			{
				LootEntryDefinition selectedEntry = SelectEntryByWeight(weightedEntries, generator);
				items.Add(CreateItemInstance(selectedEntry, generator));
				if (!selectedEntry.AllowDuplicateRolls)
				{
					weightedEntries.Remove(selectedEntry);
				}
			}
		}

		return items;
	}

	private static ItemInstance CreateItemInstance(LootEntryDefinition entry, RandomNumberGenerator random)
	{
		ItemDefinition itemDefinition = entry.Item
			?? throw new System.InvalidOperationException("Loot entry must have an item definition before generating runtime items.");
		ItemInstance item = new(itemDefinition, entry.GetRolledStackCount(random));
		item.SetAcquisitionState(ItemAcquisitionState.RunLoot);
		return item;
	}

	private static LootEntryDefinition SelectEntryByWeight(List<LootEntryDefinition> entries, RandomNumberGenerator random)
	{
		int totalWeight = 0;
		foreach (LootEntryDefinition entry in entries)
		{
			totalWeight += Mathf.Max(0, entry.Weight);
		}

		if (totalWeight <= 0)
		{
			return entries[0];
		}

		int roll = random.RandiRange(1, totalWeight);
		int cumulative = 0;
		foreach (LootEntryDefinition entry in entries)
		{
			cumulative += Mathf.Max(0, entry.Weight);
			if (roll <= cumulative)
			{
				return entry;
			}
		}

		return entries[^1];
	}

	private static LootCategoryPoolDefinition SelectPoolByWeight(List<LootCategoryPoolDefinition> pools, RandomNumberGenerator random, LootGenerationContext? context)
	{
		float totalWeight = 0.0f;
		foreach (LootCategoryPoolDefinition pool in pools)
		{
			float adjustedWeight = pool.Weight;
			if (context?.Container != null)
			{
				adjustedWeight *= ContainerModifierResolver.ResolveLootPoolWeightMultiplier(context.Requester, context.Container, pool);
			}

			totalWeight += Mathf.Max(0.0f, adjustedWeight);
		}

		if (totalWeight <= 0.0f)
		{
			return pools[0];
		}

		float roll = random.RandfRange(0.0f, totalWeight);
		float cumulative = 0.0f;
		foreach (LootCategoryPoolDefinition pool in pools)
		{
			float adjustedWeight = pool.Weight;
			if (context?.Container != null)
			{
				adjustedWeight *= ContainerModifierResolver.ResolveLootPoolWeightMultiplier(context.Requester, context.Container, pool);
			}

			cumulative += Mathf.Max(0.0f, adjustedWeight);
			if (roll <= cumulative)
			{
				return pool;
			}
		}

		return pools[^1];
	}

	private static LootEntryDefinition? SelectEntryFromPool(LootCategoryPoolDefinition pool, RandomNumberGenerator random)
	{
		List<LootEntryDefinition> weightedEntries = [];
		foreach (LootEntryDefinition entry in pool.Entries)
		{
			if (!entry.IsConfigured || entry.Weight <= 0)
			{
				continue;
			}

			weightedEntries.Add(entry);
		}

		if (weightedEntries.Count == 0)
		{
			return null;
		}

		return SelectEntryByWeight(weightedEntries, random);
	}

	private static LootRarity SelectRarityByWeight(Godot.Collections.Array<LootRarityWeightDefinition> weights, RandomNumberGenerator random, LootGenerationContext? context)
	{
		List<LootRarityWeightDefinition> configuredWeights = [];
		foreach (LootRarityWeightDefinition weight in weights)
		{
			if (weight.Rarity == LootRarity.None || weight.Weight <= 0.0f)
			{
				continue;
			}

			configuredWeights.Add(weight);
		}

		if (configuredWeights.Count == 0)
		{
			return LootRarity.White;
		}

		float totalWeight = 0.0f;
		foreach (LootRarityWeightDefinition weight in configuredWeights)
		{
			float adjustedWeight = weight.Weight;
			if (context?.Container != null)
			{
				adjustedWeight *= ContainerModifierResolver.ResolveLootRarityWeightMultiplier(context.Requester, context.Container, weight.Rarity);
			}

			totalWeight += Mathf.Max(0.0f, adjustedWeight);
		}

		if (totalWeight <= 0.0f)
		{
			return configuredWeights[0].Rarity;
		}

		float roll = random.RandfRange(0.0f, totalWeight);
		float cumulative = 0.0f;
		foreach (LootRarityWeightDefinition weight in configuredWeights)
		{
			float adjustedWeight = weight.Weight;
			if (context?.Container != null)
			{
				adjustedWeight *= ContainerModifierResolver.ResolveLootRarityWeightMultiplier(context.Requester, context.Container, weight.Rarity);
			}

			cumulative += Mathf.Max(0.0f, adjustedWeight);
			if (roll <= cumulative)
			{
				return weight.Rarity;
			}
		}

		return configuredWeights[^1].Rarity;
	}

	private static ItemCategory SelectCategoryByWeight(Godot.Collections.Array<LootCategoryWeightDefinition> weights, RandomNumberGenerator random)
	{
		List<LootCategoryWeightDefinition> configuredWeights = [];
		foreach (LootCategoryWeightDefinition weight in weights)
		{
			if (weight.Weight <= 0.0f)
			{
				continue;
			}

			configuredWeights.Add(weight);
		}

		if (configuredWeights.Count == 0)
		{
			return ItemCategory.Generic;
		}

		float totalWeight = 0.0f;
		foreach (LootCategoryWeightDefinition weight in configuredWeights)
		{
			totalWeight += Mathf.Max(0.0f, weight.Weight);
		}

		if (totalWeight <= 0.0f)
		{
			return configuredWeights[0].Category;
		}

		float roll = random.RandfRange(0.0f, totalWeight);
		float cumulative = 0.0f;
		foreach (LootCategoryWeightDefinition weight in configuredWeights)
		{
			cumulative += Mathf.Max(0.0f, weight.Weight);
			if (roll <= cumulative)
			{
				return weight.Category;
			}
		}

		return configuredWeights[^1].Category;
	}

	private static LootBucketDefinition? SelectBucket(Godot.Collections.Array<LootBucketDefinition> buckets, LootRarity rarity, ItemCategory category, RandomNumberGenerator random)
	{
		List<LootBucketDefinition> matchingBuckets = [];
		foreach (LootBucketDefinition bucket in buckets)
		{
			if (!bucket.IsConfigured || bucket.Rarity != rarity || bucket.Category != category)
			{
				continue;
			}

			matchingBuckets.Add(bucket);
		}

		return matchingBuckets.Count == 0 ? null : matchingBuckets[random.RandiRange(0, matchingBuckets.Count - 1)];
	}

	private static LootBucketDefinition? SelectAnyBucketForRarity(Godot.Collections.Array<LootBucketDefinition> buckets, LootRarity rarity, RandomNumberGenerator random)
	{
		List<LootBucketDefinition> matchingBuckets = [];
		foreach (LootBucketDefinition bucket in buckets)
		{
			if (!bucket.IsConfigured || bucket.Rarity != rarity)
			{
				continue;
			}

			matchingBuckets.Add(bucket);
		}

		return matchingBuckets.Count == 0 ? null : matchingBuckets[random.RandiRange(0, matchingBuckets.Count - 1)];
	}

	private static LootBucketDefinition? SelectAnyBucket(Godot.Collections.Array<LootBucketDefinition> buckets, RandomNumberGenerator random)
	{
		List<LootBucketDefinition> matchingBuckets = [];
		foreach (LootBucketDefinition bucket in buckets)
		{
			if (bucket.IsConfigured)
			{
				matchingBuckets.Add(bucket);
			}
		}

		return matchingBuckets.Count == 0 ? null : matchingBuckets[random.RandiRange(0, matchingBuckets.Count - 1)];
	}

	private static LootEntryDefinition? SelectEntryFromBucket(LootBucketDefinition bucket, RandomNumberGenerator random)
	{
		List<LootEntryDefinition> weightedEntries = [];
		foreach (LootEntryDefinition entry in bucket.Entries)
		{
			if (!entry.IsConfigured || entry.Weight <= 0)
			{
				continue;
			}

			weightedEntries.Add(entry);
		}

		if (weightedEntries.Count == 0)
		{
			return null;
		}

		return SelectEntryByWeight(weightedEntries, random);
	}
}
