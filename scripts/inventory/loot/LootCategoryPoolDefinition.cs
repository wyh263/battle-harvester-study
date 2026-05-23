using Godot;
using Godot.Collections;

namespace BattleHarvesterStudy.Inventory;

[GlobalClass]
public partial class LootCategoryPoolDefinition : Resource
{
	[Export]
	public string PoolId { get; set; } = "loot_pool";

	[Export]
	public string DisplayName { get; set; } = "Loot Pool";

	[Export(PropertyHint.Range, "0,999,1")]
	public int Weight { get; set; } = 1;

	[Export]
	public Array<LootEntryDefinition> Entries { get; set; } = [];

	public bool IsConfigured => Weight > 0 && Entries.Count > 0;
}
