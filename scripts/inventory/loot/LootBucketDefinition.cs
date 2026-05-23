using Godot;
using Godot.Collections;
using BattleHarvesterStudy.Items;

namespace BattleHarvesterStudy.Inventory;

[GlobalClass]
public partial class LootBucketDefinition : Resource
{
	[Export]
	public string BucketId { get; set; } = "loot_bucket";

	[Export]
	public LootRarity Rarity { get; set; } = LootRarity.White;

	[Export]
	public ItemCategory Category { get; set; } = ItemCategory.Generic;

	[Export]
	public Array<LootEntryDefinition> Entries { get; set; } = [];

	public bool IsConfigured => Entries.Count > 0;
}
