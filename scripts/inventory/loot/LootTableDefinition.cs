using Godot;
using Godot.Collections;

namespace BattleHarvesterStudy.Inventory;

[GlobalClass]
public partial class LootTableDefinition : Resource
{
	[Export]
	public string LootTableId { get; set; } = "loot_table";

	[Export]
	public string DisplayName { get; set; } = "Loot Table";

	[Export(PropertyHint.Range, "0,64,1")]
	public int MinRollCount { get; set; } = 1;

	[Export(PropertyHint.Range, "0,64,1")]
	public int MaxRollCount { get; set; } = 3;

	[Export]
	public Array<LootEntryDefinition> Entries { get; set; } = [];

	[Export]
	public Array<LootCategoryPoolDefinition> CategoryPools { get; set; } = [];

	[Export]
	public Array<LootRarityWeightDefinition> RarityWeights { get; set; } = [];

	[Export]
	public Array<LootCategoryWeightDefinition> CategoryWeights { get; set; } = [];

	[Export]
	public Array<LootBucketDefinition> Buckets { get; set; } = [];

	public bool UsesCategoryPools => CategoryPools.Count > 0;
	public bool UsesRarityBuckets => RarityWeights.Count > 0 && CategoryWeights.Count > 0 && Buckets.Count > 0;

	public int GetRollCount(RandomNumberGenerator random)
	{
		int min = Mathf.Max(0, MinRollCount);
		int max = Mathf.Max(min, MaxRollCount);
		return random.RandiRange(min, max);
	}
}
