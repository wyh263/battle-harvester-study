using Godot;

namespace BattleHarvesterStudy.Inventory;

public static class GridContainerTransferService
{
	public static bool TryQuickTransferBetweenContainers(
		GridContainerComponent from,
		GridContainerComponent to,
		string instanceId)
	{
		return InventoryTransferService.TryQuickTransfer(from, to, instanceId);
	}

	public static bool TryTransferBetweenContainers(
		GridContainerComponent from,
		GridContainerComponent to,
		string instanceId,
		Vector2I origin,
		bool rotated)
	{
		return InventoryTransferService.TryTransferToPosition(from, to, instanceId, origin, rotated);
	}
}
