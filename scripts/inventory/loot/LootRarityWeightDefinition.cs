using Godot;

namespace BattleHarvesterStudy.Inventory;

[GlobalClass]
public partial class LootRarityWeightDefinition : Resource
{
	[Export]
	public LootRarity Rarity { get; set; } = LootRarity.White;

	[Export(PropertyHint.Range, "0,9999,0.1")]
	public float Weight { get; set; } = 1.0f;
}
