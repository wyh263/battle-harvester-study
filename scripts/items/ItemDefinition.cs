using Godot;
using Godot.Collections;
using BattleHarvesterStudy.Inventory;

namespace BattleHarvesterStudy.Items;

[GlobalClass]
public partial class ItemDefinition : Resource
{
	[Export]
	public string ItemId { get; set; } = "item_generic";

	[Export]
	public string DisplayName { get; set; } = "Generic Item";

	[Export]
	public ItemCategory Category { get; set; } = ItemCategory.Generic;

	[Export(PropertyHint.Range, "1,12,1")]
	public int GridWidth { get; set; } = 1;

	[Export(PropertyHint.Range, "1,12,1")]
	public int GridHeight { get; set; } = 1;

	[Export]
	public bool CanRotate { get; set; } = true;

	[Export(PropertyHint.Range, "1,999,1")]
	public int MaxStack { get; set; } = 1;

	[Export(PropertyHint.Range, "0,999999,1")]
	public int BaseValue { get; set; } = 0;

	[Export]
	public LootRarity LootRarity { get; set; } = LootRarity.None;

	[Export]
	public Array<string> Tags { get; set; } = [];

	[Export]
	public EquipmentDefinition? Equipment { get; set; }

	[Export]
	public UsableItemDefinition? Usable { get; set; }

	[Export]
	public WeaponSkillItemDefinition? WeaponSkill { get; set; }

	[Export]
	public AmmoItemDefinition? Ammo { get; set; }
}
