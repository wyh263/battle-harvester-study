using Godot;
using Godot.Collections;
using System.Collections.Generic;

namespace BattleHarvesterStudy.Inventory;

public partial class InventoryComponent : Node
{
	[Signal]
	public delegate void InventoryChangedEventHandler();

	[Export]
	public Array<NodePath> ContainerPaths { get; set; } = [];

	private readonly List<GridContainerComponent> _containers = [];

	public IReadOnlyList<GridContainerComponent> Containers => _containers;

	public override void _Ready()
	{
		base._Ready();
		ResolveConfiguredContainers();
	}

	public bool ContainsContainer(GridContainerComponent container)
	{
		return _containers.Contains(container);
	}

	public bool TryRegisterContainer(GridContainerComponent container)
	{
		if (_containers.Contains(container))
		{
			return false;
		}

		_containers.Add(container);
		container.ContainerChanged += OnContainerChanged;
		EmitSignal(SignalName.InventoryChanged);
		return true;
	}

	public bool TryUnregisterContainer(GridContainerComponent container)
	{
		if (!_containers.Remove(container))
		{
			return false;
		}

		container.ContainerChanged -= OnContainerChanged;
		EmitSignal(SignalName.InventoryChanged);
		return true;
	}

	public GridContainerComponent? FindContainerById(string containerId)
	{
		foreach (GridContainerComponent container in _containers)
		{
			if (container.Definition?.ContainerId == containerId)
			{
				return container;
			}
		}

		return null;
	}

	public GridContainerComponent? GetPrimaryContainer()
	{
		return _containers.Count > 0 ? _containers[0] : null;
	}

	public bool TryTransferItem(GridContainerComponent from, GridContainerComponent to, string instanceId)
	{
		if (!InventoryTransferService.TryQuickTransfer(from, to, instanceId))
		{
			return false;
		}

		EmitSignal(SignalName.InventoryChanged);
		return true;
	}

	public bool TryTransferItemToAnyContainer(GridContainerComponent from, string instanceId, out GridContainerComponent? targetContainer)
	{
		targetContainer = null;
		foreach (GridContainerComponent container in _containers)
		{
			if (container == from)
			{
				continue;
			}

			if (!TryTransferItem(from, container, instanceId))
			{
				continue;
			}

			targetContainer = container;
			return true;
		}

		return false;
	}

	public bool TryTransferItemToContainerPosition(
		GridContainerComponent from,
		GridContainerComponent to,
		string instanceId,
		Vector2I origin,
		bool rotated)
	{
		if (!InventoryTransferService.TryTransferToPosition(from, to, instanceId, origin, rotated))
		{
			return false;
		}

		EmitSignal(SignalName.InventoryChanged);
		return true;
	}

	public bool TryTransferAllFrom(GridContainerComponent from, out int movedItemCount, out int failedItemCount)
	{
		movedItemCount = 0;
		failedItemCount = 0;

		if (from == null)
		{
			return false;
		}

		List<string> instanceIds = [];
		foreach (ContainerItemRecord record in from.ItemRecords)
		{
			instanceIds.Add(record.Item.InstanceId);
		}

		foreach (string instanceId in instanceIds)
		{
			if (TryTransferItemToAnyContainer(from, instanceId, out _))
			{
				movedItemCount++;
			}
			else
			{
				failedItemCount++;
			}
		}

		return movedItemCount > 0;
	}

	public bool TryAutoArrange(GridContainerComponent container)
	{
		if (!_containers.Contains(container))
		{
			return false;
		}

		bool didArrange = container.TryAutoArrange();
		if (didArrange)
		{
			EmitSignal(SignalName.InventoryChanged);
		}

		return didArrange;
	}

	public override void _ExitTree()
	{
		foreach (GridContainerComponent container in _containers)
		{
			container.ContainerChanged -= OnContainerChanged;
		}

		_containers.Clear();
		base._ExitTree();
	}

	private void ResolveConfiguredContainers()
	{
		foreach (GridContainerComponent container in _containers)
		{
			container.ContainerChanged -= OnContainerChanged;
		}

		_containers.Clear();
		foreach (NodePath containerPath in ContainerPaths)
		{
			GridContainerComponent? container = GetNodeOrNull<GridContainerComponent>(containerPath);
			if (container == null || _containers.Contains(container))
			{
				continue;
			}

			_containers.Add(container);
			container.ContainerChanged += OnContainerChanged;
		}
	}

	private void OnContainerChanged()
	{
		EmitSignal(SignalName.InventoryChanged);
	}
}
