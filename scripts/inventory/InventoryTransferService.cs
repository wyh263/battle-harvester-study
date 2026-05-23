using Godot;

namespace BattleHarvesterStudy.Inventory;

public static class InventoryTransferService
{
	public static bool TryQuickTransfer(
		GridContainerComponent from,
		GridContainerComponent to,
		string instanceId)
	{
		if (from == to || !from.ContainsItem(instanceId))
		{
			return false;
		}

		ContainerItemRecord sourceRecord = from.GetRequiredRecord(instanceId);
		if (to.TryFindAnyPlacementForItem(sourceRecord.Item, out Vector2I placementOrigin, out bool placementRotated))
		{
			return TryTransferToPosition(from, to, instanceId, placementOrigin, placementRotated);
		}

		ItemInstance movingItem = sourceRecord.Item.CreateCopy(true);
		if (!to.TryAcceptItem(movingItem))
		{
			return false;
		}

		from.RemoveItem(instanceId, out _);
		return true;
	}

	public static bool TryTransferToPosition(
		GridContainerComponent from,
		GridContainerComponent to,
		string instanceId,
		Vector2I origin,
		bool rotated)
	{
		if (from == to || !from.ContainsItem(instanceId))
		{
			return false;
		}

		ContainerItemRecord sourceRecord = from.GetRequiredRecord(instanceId);
		ItemInstance movingItem = sourceRecord.Item.CreateCopy(true);
		if (!to.TryPlaceIncomingItemAt(movingItem, origin, rotated))
		{
			int mergeAmount = to.GetMergeAmountAt(origin, sourceRecord.Item);
			if (mergeAmount <= 0)
			{
				return false;
			}

			if (!to.TryMergeIncomingItemAt(origin, sourceRecord.Item, out int mergedAmount) || mergedAmount <= 0)
			{
				return false;
			}

			return from.TryConsumeItemStack(instanceId, mergedAmount);
		}

		from.RemoveItem(instanceId, out _);
		return true;
	}

	public static bool TryRepositionWithinContainer(
		GridContainerComponent container,
		string instanceId,
		Vector2I origin,
		bool rotated)
	{
		if (!container.ContainsItem(instanceId))
		{
			return false;
		}

		ContainerItemRecord sourceRecord = container.GetRequiredRecord(instanceId);
		if (container.TryMoveItem(instanceId, origin, rotated))
		{
			return true;
		}

		int mergeAmount = container.GetMergeAmountAt(origin, sourceRecord.Item, instanceId);
		if (mergeAmount <= 0
			|| !container.TryMergeIncomingItemAt(origin, sourceRecord.Item, out int mergedAmount, instanceId))
		{
			return false;
		}

		return container.TryConsumeItemStack(instanceId, mergedAmount);
	}
}
