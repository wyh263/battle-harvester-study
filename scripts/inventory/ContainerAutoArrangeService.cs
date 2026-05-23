using Godot;
using System.Collections.Generic;

namespace BattleHarvesterStudy.Inventory;

public static class ContainerAutoArrangeService
{
	public static Dictionary<string, Vector2I>? BuildArrangement(GridContainerDefinition definition, IEnumerable<ItemInstance> items)
	{
		List<ItemInstance> sortedItems = new(items);
		sortedItems.Sort(CompareItems);

		Dictionary<string, Vector2I> placements = [];
		bool[,] occupied = new bool[definition.GridColumns, definition.GridRows];

		foreach (ItemInstance item in sortedItems)
		{
			if (!TryPlaceItem(definition, occupied, item, out Vector2I origin))
			{
				return null;
			}

			placements[item.InstanceId] = origin;
		}

		return placements;
	}

	private static bool TryPlaceItem(GridContainerDefinition definition, bool[,] occupied, ItemInstance item, out Vector2I origin)
	{
		Vector2I footprint = item.GetFootprint();
		for (int y = 0; y <= definition.GridRows - footprint.Y; y++)
		{
			for (int x = 0; x <= definition.GridColumns - footprint.X; x++)
			{
				Vector2I candidate = new(x, y);
				if (!CanOccupy(candidate, footprint, occupied))
				{
					continue;
				}

				MarkOccupied(candidate, footprint, occupied, true);
				origin = candidate;
				return true;
			}
		}

		origin = Vector2I.Zero;
		return false;
	}

	private static bool CanOccupy(Vector2I origin, Vector2I footprint, bool[,] occupied)
	{
		for (int y = 0; y < footprint.Y; y++)
		{
			for (int x = 0; x < footprint.X; x++)
			{
				if (occupied[origin.X + x, origin.Y + y])
				{
					return false;
				}
			}
		}

		return true;
	}

	private static void MarkOccupied(Vector2I origin, Vector2I footprint, bool[,] occupied, bool value)
	{
		for (int y = 0; y < footprint.Y; y++)
		{
			for (int x = 0; x < footprint.X; x++)
			{
				occupied[origin.X + x, origin.Y + y] = value;
			}
		}
	}

	private static int CompareItems(ItemInstance a, ItemInstance b)
	{
		int areaComparison = (b.GetFootprintWidth() * b.GetFootprintHeight()).CompareTo(a.GetFootprintWidth() * a.GetFootprintHeight());
		if (areaComparison != 0)
		{
			return areaComparison;
		}

		int widthComparison = b.GetFootprintWidth().CompareTo(a.GetFootprintWidth());
		if (widthComparison != 0)
		{
			return widthComparison;
		}

		return string.CompareOrdinal(a.Definition.ItemId, b.Definition.ItemId);
	}
}
