using Godot;
using BattleHarvesterStudy.Items;

namespace BattleHarvesterStudy.Inventory;

public sealed class ContainerItemRecord
{
	public required ItemInstance Item { get; init; }
	public required Vector2I Origin { get; set; }

	public Rect2I GetOccupiedRect()
	{
		return new Rect2I(Origin, Item.GetFootprint());
	}
}
