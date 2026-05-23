using Godot;
using System.Collections.Generic;

namespace BattleHarvesterStudy.Inventory.Search;

public static class ContainerModifierResolver
{
	public static float ResolveSearchSpeedMultiplier(Node? requester, WorldContainer container)
	{
		float multiplier = 1.0f;
		foreach (IContainerSearchLootModifierSource source in EnumerateModifierSources(requester))
		{
			multiplier *= Mathf.Max(0.01f, source.GetSearchSpeedMultiplier(container));
		}

		return Mathf.Max(0.01f, multiplier);
	}

	public static float ResolveLootPoolWeightMultiplier(Node? requester, WorldContainer container, LootCategoryPoolDefinition pool)
	{
		float multiplier = 1.0f;
		foreach (IContainerSearchLootModifierSource source in EnumerateModifierSources(requester))
		{
			multiplier *= Mathf.Max(0.0f, source.GetLootPoolWeightMultiplier(container, pool));
		}

		return Mathf.Max(0.0f, multiplier);
	}

	public static float ResolveLootRarityWeightMultiplier(Node? requester, WorldContainer container, LootRarity rarity)
	{
		float multiplier = 1.0f;
		foreach (IContainerSearchLootModifierSource source in EnumerateModifierSources(requester))
		{
			multiplier *= Mathf.Max(0.0f, source.GetLootRarityWeightMultiplier(container, rarity));
		}

		return Mathf.Max(0.0f, multiplier);
	}

	private static IEnumerable<IContainerSearchLootModifierSource> EnumerateModifierSources(Node? root)
	{
		if (root == null)
		{
			yield break;
		}

		Stack<Node> stack = new();
		stack.Push(root);
		while (stack.Count > 0)
		{
			Node node = stack.Pop();
			if (node is IContainerSearchLootModifierSource source)
			{
				yield return source;
			}

			foreach (Node child in node.GetChildren())
			{
				stack.Push(child);
			}
		}
	}
}
