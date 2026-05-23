using Godot;
using BattleHarvesterStudy.Inventory;

namespace BattleHarvesterStudy.Inventory.Search;

[GlobalClass]
public partial class ContainerModifierProfileDefinition : Resource
{
	[Export]
	public string ModifierId { get; set; } = "container_modifier_generic";

	[Export]
	public string DisplayName { get; set; } = "Generic Container Modifier";

	[Export(PropertyHint.Range, "0.1,10.0,0.1")]
	public float GlobalSearchSpeedMultiplier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0.1,10.0,0.1")]
	public float GlobalLootWeightMultiplier { get; set; } = 1.0f;

	[ExportGroup("Loot Rarity Weight")]
	[Export(PropertyHint.Range, "0.0,10.0,0.1")]
	public float WhiteLootWeightMultiplier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0.0,10.0,0.1")]
	public float GreenLootWeightMultiplier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0.0,10.0,0.1")]
	public float BlueLootWeightMultiplier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0.0,10.0,0.1")]
	public float PurpleLootWeightMultiplier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0.0,10.0,0.1")]
	public float GoldLootWeightMultiplier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0.0,10.0,0.1")]
	public float RedLootWeightMultiplier { get; set; } = 1.0f;

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

	public float ResolveSearchSpeedMultiplier(GridContainerDefinition? definition)
	{
		float multiplier = GlobalSearchSpeedMultiplier;
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

	public float ResolveLootPoolWeightMultiplier(LootCategoryPoolDefinition pool)
	{
		float multiplier = GlobalLootWeightMultiplier;
		multiplier *= pool.PoolId switch
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

		return multiplier;
	}

	public float ResolveLootRarityWeightMultiplier(LootRarity rarity)
	{
		return GlobalLootWeightMultiplier * rarity switch
		{
			LootRarity.White => WhiteLootWeightMultiplier,
			LootRarity.Green => GreenLootWeightMultiplier,
			LootRarity.Blue => BlueLootWeightMultiplier,
			LootRarity.Purple => PurpleLootWeightMultiplier,
			LootRarity.Gold => GoldLootWeightMultiplier,
			LootRarity.Red => RedLootWeightMultiplier,
			_ => 1.0f,
		};
	}
}
