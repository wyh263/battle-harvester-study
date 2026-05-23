using Godot;
using System;
using System.Collections.Generic;
using BattleHarvesterStudy.Inventory.Search;

namespace BattleHarvesterStudy.Inventory;

public partial class GridContainerComponent : Node
{
	[Signal]
	public delegate void ContainerChangedEventHandler();

	private sealed class PlannedStackMerge
	{
		public required ContainerItemRecord Record { get; init; }
		public required int Amount { get; init; }
		public required int RunLootAmount { get; init; }
	}

	[Export]
	public GridContainerDefinition? Definition { get; set; }

	public ContainerSearchRuntimeComponent? SearchRuntime { get; set; }

	private readonly Dictionary<string, ContainerItemRecord> _items = [];

	public IReadOnlyCollection<ContainerItemRecord> ItemRecords => _items.Values;

	public bool IsConfigured => Definition != null;

	public Vector2I GetGridSize()
	{
		return Definition == null ? Vector2I.Zero : new Vector2I(Definition.GridColumns, Definition.GridRows);
	}

	public bool CanPlaceItem(ItemInstance item, Vector2I origin, string? ignoreInstanceId = null)
	{
		if (Definition == null)
		{
			return false;
		}

		if (!Definition.AcceptsCategory(item.Definition.Category))
		{
			return false;
		}

		Vector2I footprint = item.GetFootprint();
		if (origin.X < 0 || origin.Y < 0
			|| origin.X + footprint.X > Definition.GridColumns
			|| origin.Y + footprint.Y > Definition.GridRows)
		{
			return false;
		}

		Rect2I candidateRect = new(origin, footprint);
		foreach (ContainerItemRecord record in _items.Values)
		{
			if (record.Item.InstanceId == ignoreInstanceId)
			{
				continue;
			}

			if (record.GetOccupiedRect().Intersects(candidateRect))
			{
				return false;
			}
		}

		return true;
	}

	public bool PlaceItem(ItemInstance item, Vector2I origin)
	{
		if (_items.ContainsKey(item.InstanceId) || !CanPlaceItem(item, origin))
		{
			return false;
		}

		_items[item.InstanceId] = new ContainerItemRecord
		{
			Item = item,
			Origin = origin
		};
		EmitSignal(SignalName.ContainerChanged);
		return true;
	}

	public bool TryAcceptItem(ItemInstance item)
	{
		if (Definition == null)
		{
			return false;
		}

		int remainingCount = item.StackCount;
		int remainingRunLootCount = item.RunLootStackCount;
		List<PlannedStackMerge> plannedMerges = BuildPlannedStackMerges(item, ref remainingCount);
		foreach (PlannedStackMerge merge in plannedMerges)
		{
			remainingRunLootCount = Mathf.Max(0, remainingRunLootCount - merge.RunLootAmount);
		}
		ItemInstance? remainderItem = null;
		Vector2I placementOrigin = Vector2I.Zero;

		if (remainingCount > 0)
		{
			remainderItem = item.CreateCopy(true);
			remainderItem.TrySetStackCount(remainingCount);
			remainderItem.RestoreAcquisitionState(
				remainingRunLootCount > 0 ? ItemAcquisitionState.RunLoot : ItemAcquisitionState.Base,
				remainingRunLootCount);
			if (!TryFindAnyPlacement(remainderItem, out placementOrigin, out bool rotated))
			{
				return false;
			}

			remainderItem.TrySetRotation(rotated);
		}

		ApplyPlannedMerges(plannedMerges);
		if (remainderItem != null)
		{
			_items[remainderItem.InstanceId] = new ContainerItemRecord
			{
				Item = remainderItem,
				Origin = placementOrigin
			};
		}

		if (plannedMerges.Count > 0 || remainderItem != null)
		{
			EmitSignal(SignalName.ContainerChanged);
		}

		return true;
	}

	public bool TryPlaceIncomingItemAt(ItemInstance item, Vector2I origin, bool rotated)
	{
		if (Definition == null)
		{
			return false;
		}

		ItemInstance placedItem = item.CreateCopy(true);
		if (!placedItem.TrySetRotation(rotated))
		{
			return false;
		}

		if (!CanPlaceItem(placedItem, origin))
		{
			return false;
		}

		_items[placedItem.InstanceId] = new ContainerItemRecord
		{
			Item = placedItem,
			Origin = origin
		};
		EmitSignal(SignalName.ContainerChanged);
		return true;
	}

	public int GetMergeAmountAt(Vector2I cell, ItemInstance item, string? ignoreInstanceId = null)
	{
		ContainerItemRecord? targetRecord = FindItemAt(cell);
		if (targetRecord == null || targetRecord.Item.InstanceId == ignoreInstanceId)
		{
			return 0;
		}

		if (!targetRecord.Item.CanStackWith(item))
		{
			return 0;
		}

		return Mathf.Min(item.StackCount, targetRecord.Item.GetAvailableStackSpace());
	}

	public bool TryMergeIncomingItemAt(Vector2I cell, ItemInstance item, out int mergedAmount, string? ignoreInstanceId = null)
	{
		mergedAmount = 0;
		ContainerItemRecord? targetRecord = FindItemAt(cell);
		if (targetRecord == null || targetRecord.Item.InstanceId == ignoreInstanceId)
		{
			return false;
		}

		if (!targetRecord.Item.CanStackWith(item))
		{
			return false;
		}

		if (!targetRecord.Item.TryAddToStack(item.StackCount, out mergedAmount) || mergedAmount <= 0)
		{
			return false;
		}

		targetRecord.Item.AddRunLootToStack(item.PeekRunLootAmountInConsumedStack(mergedAmount));

		EmitSignal(SignalName.ContainerChanged);
		return true;
	}

	public bool TryConsumeItemStack(string instanceId, int amount)
	{
		if (amount <= 0 || !_items.TryGetValue(instanceId, out ContainerItemRecord? record))
		{
			return false;
		}

		if (amount >= record.Item.StackCount)
		{
			_items.Remove(instanceId);
			EmitSignal(SignalName.ContainerChanged);
			return true;
		}

		if (!record.Item.TryConsumeStackAmount(amount))
		{
			return false;
		}

		EmitSignal(SignalName.ContainerChanged);
		return true;
	}

	public bool TryConsumeItemUse(string instanceId)
	{
		if (!_items.TryGetValue(instanceId, out ContainerItemRecord? record))
		{
			return false;
		}

		if (!record.Item.TryConsumeUse(out bool depleted))
		{
			return false;
		}

		if (depleted)
		{
			_items.Remove(instanceId);
		}

		EmitSignal(SignalName.ContainerChanged);
		return true;
	}

	public bool RemoveItem(string instanceId, out ItemInstance? removedItem)
	{
		removedItem = null;
		if (!_items.Remove(instanceId, out ContainerItemRecord? record))
		{
			return false;
		}

		removedItem = record.Item;
		EmitSignal(SignalName.ContainerChanged);
		return true;
	}

	public bool TryMoveItem(string instanceId, Vector2I newOrigin, bool rotated)
	{
		if (!_items.TryGetValue(instanceId, out ContainerItemRecord? record))
		{
			return false;
		}

		bool originalRotation = record.Item.IsRotated;
		if (rotated != originalRotation)
		{
			record.Item.Rotate();
		}

		if (!CanPlaceItem(record.Item, newOrigin, instanceId))
		{
			if (rotated != originalRotation)
			{
				record.Item.Rotate();
			}

			return false;
		}

		record.Origin = newOrigin;
		EmitSignal(SignalName.ContainerChanged);
		return true;
	}

	public ContainerItemRecord? FindItemAt(Vector2I cell)
	{
		foreach (ContainerItemRecord record in _items.Values)
		{
			if (record.GetOccupiedRect().HasPoint(cell))
			{
				return record;
			}
		}

		return null;
	}

	public bool ContainsItem(string instanceId)
	{
		return _items.ContainsKey(instanceId);
	}

	public bool TryAutoArrange()
	{
		if (Definition == null || !Definition.AutoSortEnabled)
		{
			return false;
		}

		List<ItemInstance> items = [];
		foreach (ContainerItemRecord record in _items.Values)
		{
			items.Add(record.Item);
		}

		Dictionary<string, Vector2I>? arrangement = ContainerAutoArrangeService.BuildArrangement(Definition, items);
		if (arrangement == null)
		{
			return false;
		}

		foreach ((string instanceId, Vector2I origin) in arrangement)
		{
			if (_items.TryGetValue(instanceId, out ContainerItemRecord? record))
			{
				record.Origin = origin;
			}
		}

		EmitSignal(SignalName.ContainerChanged);
		return true;
	}

	public void NotifyVisualStateChanged()
	{
		EmitSignal(SignalName.ContainerChanged);
	}

	public void Clear()
	{
		if (_items.Count == 0)
		{
			return;
		}

		_items.Clear();
		EmitSignal(SignalName.ContainerChanged);
	}

	public bool TryAddItemAnywhere(ItemInstance item)
	{
		if (!TryFindAnyPlacement(item, out Vector2I origin, out bool rotated))
		{
			return false;
		}

		item.TrySetRotation(rotated);
		return PlaceItem(item, origin);
	}

	public bool TryFindAnyPlacementForItem(ItemInstance item, out Vector2I origin, out bool rotated)
	{
		ItemInstance probeItem = item.CreateCopy();
		return TryFindAnyPlacement(probeItem, out origin, out rotated);
	}

	public ContainerItemRecord GetRequiredRecord(string instanceId)
	{
		if (_items.TryGetValue(instanceId, out ContainerItemRecord? record))
		{
			return record;
		}

		throw new InvalidOperationException($"Container does not contain item instance: {instanceId}");
	}

	private List<PlannedStackMerge> BuildPlannedStackMerges(ItemInstance item, ref int remainingCount)
	{
		List<PlannedStackMerge> plannedMerges = [];
		if (item.Definition.MaxStack <= 1)
		{
			return plannedMerges;
		}

		int consumedCount = 0;

		foreach (ContainerItemRecord record in _items.Values)
		{
			if (!record.Item.CanStackWith(item))
			{
				continue;
			}

			int stackSpace = record.Item.GetAvailableStackSpace();
			if (stackSpace <= 0)
			{
				continue;
			}

			int amountToMerge = Mathf.Min(stackSpace, remainingCount);
			if (amountToMerge <= 0)
			{
				continue;
			}

			plannedMerges.Add(new PlannedStackMerge
			{
				Record = record,
				Amount = amountToMerge,
				RunLootAmount = item.PeekRunLootAmountInConsumedStack(consumedCount + amountToMerge)
					- item.PeekRunLootAmountInConsumedStack(consumedCount)
			});
			remainingCount -= amountToMerge;
			consumedCount += amountToMerge;

			if (remainingCount == 0)
			{
				break;
			}
		}

		return plannedMerges;
	}

	private void ApplyPlannedMerges(List<PlannedStackMerge> plannedMerges)
	{
		foreach (PlannedStackMerge merge in plannedMerges)
		{
			merge.Record.Item.TryAddToStack(merge.Amount, out _);
			merge.Record.Item.AddRunLootToStack(merge.RunLootAmount);
		}
	}

	private bool TryFindAnyPlacement(ItemInstance item, out Vector2I origin, out bool rotated)
	{
		origin = Vector2I.Zero;
		rotated = item.IsRotated;

		if (Definition == null)
		{
			return false;
		}

		if (TryFindPlacementForCurrentRotation(item, out origin))
		{
			return true;
		}

		if (!item.Definition.CanRotate || item.Definition.GridWidth == item.Definition.GridHeight)
		{
			return false;
		}

		item.Rotate();
		bool foundRotatedPlacement = TryFindPlacementForCurrentRotation(item, out origin);
		item.Rotate();
		if (!foundRotatedPlacement)
		{
			return false;
		}

		rotated = !rotated;
		return true;
	}

	private bool TryFindPlacementForCurrentRotation(ItemInstance item, out Vector2I origin)
	{
		origin = Vector2I.Zero;
		if (Definition == null)
		{
			return false;
		}

		for (int y = 0; y < Definition.GridRows; y++)
		{
			for (int x = 0; x < Definition.GridColumns; x++)
			{
				Vector2I candidateOrigin = new(x, y);
				if (!CanPlaceItem(item, candidateOrigin))
				{
					continue;
				}

				origin = candidateOrigin;
				return true;
			}
		}

		return false;
	}
}
