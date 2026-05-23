using Godot;
using Godot.Collections;
using System.Collections.Generic;

namespace BattleHarvesterStudy.Inventory.Search;

public partial class ActiveContainerModifierComponent : Node, IContainerSearchLootModifierSource
{
	[Signal]
	public delegate void ActiveModifiersChangedEventHandler();

	private readonly System.Collections.Generic.Dictionary<string, ContainerModifierProfileDefinition> _activeProfiles = [];

	public int ActiveModifierCount => _activeProfiles.Count;

	public bool ApplyProfile(ContainerModifierProfileDefinition? profile)
	{
		if (profile == null)
		{
			return false;
		}

		string key = string.IsNullOrWhiteSpace(profile.ModifierId)
			? profile.GetInstanceId().ToString()
			: profile.ModifierId;

		_activeProfiles[key] = profile;
		EmitSignal(SignalName.ActiveModifiersChanged);
		return true;
	}

	public bool RemoveProfile(string modifierId)
	{
		if (!_activeProfiles.Remove(modifierId))
		{
			return false;
		}

		EmitSignal(SignalName.ActiveModifiersChanged);
		return true;
	}

	public Array<string> GetActiveModifierNames()
	{
		Array<string> names = [];
		foreach (ContainerModifierProfileDefinition profile in _activeProfiles.Values)
		{
			names.Add(profile.DisplayName);
		}

		return names;
	}

	public IReadOnlyList<ContainerModifierProfileDefinition> GetActiveProfiles()
	{
		return new List<ContainerModifierProfileDefinition>(_activeProfiles.Values);
	}

	public float GetGlobalSearchSpeedMultiplier()
	{
		float multiplier = 1.0f;
		foreach (ContainerModifierProfileDefinition profile in _activeProfiles.Values)
		{
			multiplier *= profile.GlobalSearchSpeedMultiplier;
		}

		return multiplier;
	}

	public float GetGlobalLootWeightMultiplier()
	{
		float multiplier = 1.0f;
		foreach (ContainerModifierProfileDefinition profile in _activeProfiles.Values)
		{
			multiplier *= profile.GlobalLootWeightMultiplier;
		}

		return multiplier;
	}

	public float GetGlobalLootRarityWeightMultiplier(LootRarity rarity)
	{
		float multiplier = 1.0f;
		foreach (ContainerModifierProfileDefinition profile in _activeProfiles.Values)
		{
			multiplier *= profile.ResolveLootRarityWeightMultiplier(rarity);
		}

		return multiplier;
	}

	public float GetSearchSpeedMultiplier(WorldContainer container)
	{
		float multiplier = 1.0f;
		GridContainerDefinition? definition = container.GetGridContainer()?.Definition;
		foreach (ContainerModifierProfileDefinition profile in _activeProfiles.Values)
		{
			multiplier *= profile.ResolveSearchSpeedMultiplier(definition);
		}

		return multiplier;
	}

	public float GetLootPoolWeightMultiplier(WorldContainer container, LootCategoryPoolDefinition pool)
	{
		float multiplier = 1.0f;
		foreach (ContainerModifierProfileDefinition profile in _activeProfiles.Values)
		{
			multiplier *= profile.ResolveLootPoolWeightMultiplier(pool);
		}

		return multiplier;
	}

	public float GetLootRarityWeightMultiplier(WorldContainer container, LootRarity rarity)
	{
		float multiplier = 1.0f;
		foreach (ContainerModifierProfileDefinition profile in _activeProfiles.Values)
		{
			multiplier *= profile.ResolveLootRarityWeightMultiplier(rarity);
		}

		return multiplier;
	}
}
