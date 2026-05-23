using Godot;

namespace BattleHarvesterStudy.Inventory;

public sealed class LootGenerationContext
{
	public Node3D? Requester { get; init; }
	public WorldContainer? Container { get; init; }
}
