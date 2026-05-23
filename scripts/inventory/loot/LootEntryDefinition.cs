using Godot;

namespace BattleHarvesterStudy.Inventory;

[GlobalClass]
public partial class LootEntryDefinition : Resource
{
	[Export]
	public string EntryId { get; set; } = "loot_entry";

	[Export]
	public ItemDefinition? Item { get; set; }

	[Export(PropertyHint.Range, "1,999,1")]
	public int Weight { get; set; } = 1;

	[Export(PropertyHint.Range, "1,999,1")]
	public int MinStackCount { get; set; } = 1;

	[Export(PropertyHint.Range, "1,999,1")]
	public int MaxStackCount { get; set; } = 1;

	[Export]
	public bool GuaranteedDrop { get; set; }

	[Export]
	public bool AllowDuplicateRolls { get; set; } = true;

	public bool IsConfigured => Item != null;

	public int GetRolledStackCount(RandomNumberGenerator random)
	{
		int min = Mathf.Max(1, MinStackCount);
		int max = Mathf.Max(min, MaxStackCount);
		int clampedMaxStack = Mathf.Max(1, Item?.MaxStack ?? max);
		min = Mathf.Min(min, clampedMaxStack);
		max = Mathf.Min(max, clampedMaxStack);
		return random.RandiRange(min, max);
	}
}
