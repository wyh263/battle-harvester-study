using Godot;
using BattleHarvesterStudy.Items;

namespace BattleHarvesterStudy.Inventory;

[GlobalClass]
public partial class LootCategoryWeightDefinition : Resource
{
	[Export]
	public ItemCategory Category { get; set; } = ItemCategory.Generic;

	[Export(PropertyHint.Range, "0,9999,0.1")]
	public float Weight { get; set; } = 1.0f;
}
