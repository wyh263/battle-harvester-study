using Godot;
using BattleHarvesterStudy.Inventory;

namespace BattleHarvesterStudy.Inventory.Search;

public partial class DefaultContainerModifierComponent : Node, IContainerSearchLootModifierSource
{
	[Export(PropertyHint.Range, "0.1,10.0,0.1")]
	public float GlobalSearchSpeedMultiplier { get; set; } = 1.0f;

	[ExportGroup("Search Speed By Tag")]
	[Export(PropertyHint.Range, "0.1,10.0,0.1")]
	public float MedicalSearchSpeedMultiplier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0.1,10.0,0.1")]
	public float MilitarySearchSpeedMultiplier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0.1,10.0,0.1")]
	public float TechSearchSpeedMultiplier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0.1,10.0,0.1")]
	public float CivilianSearchSpeedMultiplier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0.1,10.0,0.1")]
	public float WeaponSearchSpeedMultiplier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0.1,10.0,0.1")]
	public float ValuableSearchSpeedMultiplier { get; set; } = 1.0f;

	[ExportGroup("Loot Pool Weight")]
	[Export(PropertyHint.Range, "0.0,10.0,0.1")]
	public float MiscPoolWeightMultiplier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0.0,10.0,0.1")]
	public float MedicalPoolWeightMultiplier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0.0,10.0,0.1")]
	public float SupportPoolWeightMultiplier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0.0,10.0,0.1")]
	public float EquipmentPoolWeightMultiplier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0.0,10.0,0.1")]
	public float ManualPoolWeightMultiplier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0.0,10.0,0.1")]
	public float TechPoolWeightMultiplier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0.0,10.0,0.1")]
	public float ValuablePoolWeightMultiplier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0.0,10.0,0.1")]
	public float FindPoolWeightMultiplier { get; set; } = 1.0f;

	public float GetSearchSpeedMultiplier(WorldContainer container)
	{
		float multiplier = GlobalSearchSpeedMultiplier;
		GridContainerDefinition? definition = container.GetGridContainer()?.Definition;
		if (definition == null)
		{
			return multiplier;
		}

		if (definition.HasTag(ContainerTag.Medical))
		{
			multiplier *= MedicalSearchSpeedMultiplier;
		}

		if (definition.HasTag(ContainerTag.Military))
		{
			multiplier *= MilitarySearchSpeedMultiplier;
		}

		if (definition.HasTag(ContainerTag.Tech))
		{
			multiplier *= TechSearchSpeedMultiplier;
		}

		if (definition.HasTag(ContainerTag.Civilian))
		{
			multiplier *= CivilianSearchSpeedMultiplier;
		}

		if (definition.HasTag(ContainerTag.Weapon))
		{
			multiplier *= WeaponSearchSpeedMultiplier;
		}

		if (definition.HasTag(ContainerTag.Valuable))
		{
			multiplier *= ValuableSearchSpeedMultiplier;
		}

		return multiplier;
	}

	public float GetLootPoolWeightMultiplier(WorldContainer container, LootCategoryPoolDefinition pool)
	{
		return pool.PoolId switch
		{
			"misc" => MiscPoolWeightMultiplier,
			"medical" => MedicalPoolWeightMultiplier,
			"support" => SupportPoolWeightMultiplier,
			"equipment" => EquipmentPoolWeightMultiplier,
			"manual" => ManualPoolWeightMultiplier,
			"tech" => TechPoolWeightMultiplier,
			"valuable" => ValuablePoolWeightMultiplier,
			"find" => FindPoolWeightMultiplier,
			_ => 1.0f,
		};
	}

	public float GetLootRarityWeightMultiplier(WorldContainer container, LootRarity rarity)
	{
		return 1.0f;
	}
}
