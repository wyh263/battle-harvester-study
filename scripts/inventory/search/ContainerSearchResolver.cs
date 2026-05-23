using BattleHarvesterStudy.Items;

namespace BattleHarvesterStudy.Inventory.Search;

public static class ContainerSearchResolver
{
	public static float ResolveSearchDurationSeconds(ItemDefinition definition, float searchSpeedMultiplier = 1.0f)
	{
		float baseDuration = LootRarityResolver.Resolve(definition) switch
		{
			LootRarity.White => 0.5f,
			LootRarity.Green => 0.5f,
			LootRarity.Blue => 1.0f,
			LootRarity.Purple => 1.5f,
			LootRarity.Gold => 2.0f,
			LootRarity.Red => 4.0f,
			_ => 1.0f,
		};

		return baseDuration / System.MathF.Max(0.01f, searchSpeedMultiplier);
	}
}
