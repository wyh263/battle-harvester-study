using Godot;
using Godot.Collections;
using BattleHarvesterStudy.Items;
using BattleHarvesterStudy.Session;
using BattleHarvesterStudy.Inventory.Search;

namespace BattleHarvesterStudy.Presentation;

public partial class InventoryInteractionController : Node
{
	[Signal]
	public delegate void StatusTextChangedEventHandler(string textKey, Godot.Collections.Dictionary<string, Variant> formatArgs);

	[Signal]
	public delegate void InteractionStateChangedEventHandler();

	private const string DefaultTakeAllAction = "take_all";
	private const string DefaultStashAllAction = "stash_transfer_all";
	private const string DefaultRotatePreviewAction = "container_rotate_preview";

	public enum GridSide
	{
		None = 0,
		Container = 1,
		Player = 2,
		Equipment = 3,
		Secure = 4,
	}

	public readonly record struct DetailTarget(GridSide Side, string InstanceId, EquipmentSlotType EquipmentSlot);

	private sealed class DragState
	{
		public required GridSide SourceSide { get; init; }
		public GridContainerComponent? SourceContainer { get; init; }
		public EquipmentSlotType SourceEquipmentSlot { get; init; } = EquipmentSlotType.None;
		public required string InstanceId { get; init; }
		public Vector2I SourceOrigin { get; init; } = Vector2I.Zero;
		public bool PreviewRotated { get; set; }
		public Vector2I HoverCell { get; set; } = Vector2I.Zero;
		public GridSide HoverSide { get; set; } = GridSide.None;
		public EquipmentSlotType HoverEquipmentSlot { get; set; } = EquipmentSlotType.None;
		public DetailTarget? HoverDetailTarget { get; set; }
		public int HoverWeaponSkillSlotIndex { get; set; } = -1;
	}

	[Export]
	public string TakeAllAction { get; set; } = DefaultTakeAllAction;

	[Export]
	public string StashAllAction { get; set; } = DefaultStashAllAction;

	[Export]
	public string RotatePreviewAction { get; set; } = DefaultRotatePreviewAction;

	[Export]
	public NodePath UiContextPath { get; set; } = new("../PlayerUiContext");

	[Export]
	public NodePath UiControllerPath { get; set; } = new("../InventoryUiController");

	[Export]
	public NodePath EquipmentPanelPresenterPath { get; set; } = new("../EquipmentPanelPresenter");

	private Node3D? _gameplayRoot;
	private InventoryUiController? _uiController;
	private InventoryComponent? _playerInventory;
	private InventoryGridView? _playerGridView;
	private InventoryGridView? _containerGridView;
	private InventoryGridView? _secureGridView;
	private EquipmentComponent? _equipmentComponent;
	private EquipmentPanelPresenter? _equipmentPanelPresenter;
	private ItemScreenCoordinator? _itemScreenCoordinator;
	private PlayerUiContext? _uiContext;
	private Vector2I _playerSelectedCell = Vector2I.Zero;
	private Vector2I _containerSelectedCell = Vector2I.Zero;
	private Vector2I _secureSelectedCell = Vector2I.Zero;
	private GridSide _focusedSide = GridSide.Player;
	private DragState? _dragState;
	private GridContainerComponent? _lastKnownContainer;
	private readonly System.Collections.Generic.List<DetailTarget> _detailTargets = [];
	private bool _warehouseSellModeActive;
	private readonly System.Collections.Generic.HashSet<string> _selectedWarehouseSaleItemIds = [];
	private bool _customFailureStatusEmitted;

	public GridSide FocusedSide => _focusedSide;
	public bool HasActiveDrag => _dragState != null;
	public System.Collections.Generic.IReadOnlyList<DetailTarget> DetailTargets => _detailTargets;
	public bool WarehouseSellModeActive => _warehouseSellModeActive;
	public System.Collections.Generic.IReadOnlyCollection<string> SelectedWarehouseSaleItemIds => _selectedWarehouseSaleItemIds;

	public void BindDependencies(
		Node3D? gameplayRoot,
		PlayerUiContext? uiContext,
		InventoryUiController? uiController,
		InventoryGridView? playerGridView,
		InventoryGridView? containerGridView,
		InventoryGridView? secureGridView,
		EquipmentPanelPresenter? equipmentPanelPresenter,
		ItemScreenCoordinator? itemScreenCoordinator)
	{
		_gameplayRoot = gameplayRoot;
		_uiContext = uiContext;
		_uiController = uiController;
		_playerInventory = _uiContext?.Inventory;
		_playerGridView = playerGridView;
		_containerGridView = containerGridView;
		_secureGridView = secureGridView;
		_equipmentComponent = _uiContext?.Equipment;
		_equipmentPanelPresenter = equipmentPanelPresenter;
		_itemScreenCoordinator = itemScreenCoordinator;
	}

	public override void _Ready()
	{
		_gameplayRoot ??= UiNodeLocator.ResolveGameplayRoot(this);
		_uiContext ??= GetNodeOrNull<PlayerUiContext>(UiContextPath);
		_uiController ??= GetNodeOrNull<InventoryUiController>(UiControllerPath);
		_playerInventory ??= _uiContext?.Inventory;
		_equipmentComponent ??= _uiContext?.Equipment;
		_equipmentPanelPresenter ??= GetNodeOrNull<EquipmentPanelPresenter>(EquipmentPanelPresenterPath);
		_itemScreenCoordinator ??= GetNodeOrNull<ItemScreenCoordinator>("../ItemScreenCoordinator");

		if (_uiController != null)
		{
			_uiController.UiStateChanged += OnUiStateChanged;
		}
		if (_equipmentPanelPresenter != null)
		{
			_equipmentPanelPresenter.SlotPressed += OnEquipmentSlotPressed;
			_equipmentPanelPresenter.SlotDoublePressed += OnEquipmentSlotDoublePressed;
		}

		SetProcess(true);
		SetProcessInput(true);
	}

	public override void _ExitTree()
	{
		if (_uiController != null)
		{
			_uiController.UiStateChanged -= OnUiStateChanged;
		}
		if (_equipmentPanelPresenter != null)
		{
			_equipmentPanelPresenter.SlotPressed -= OnEquipmentSlotPressed;
			_equipmentPanelPresenter.SlotDoublePressed -= OnEquipmentSlotDoublePressed;
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (_dragState == null)
		{
			return;
		}

		if (@event.IsActionPressed(RotatePreviewAction))
		{
			_dragState.PreviewRotated = !_dragState.PreviewRotated;
			NotifyInteractionStateChanged();
			GetViewport().SetInputAsHandled();
			return;
		}

		if (@event is not InputEventMouseButton mouseButton)
		{
			return;
		}

		if (mouseButton.ButtonIndex == MouseButton.Right && mouseButton.Pressed)
		{
			_dragState.PreviewRotated = !_dragState.PreviewRotated;
			NotifyInteractionStateChanged();
			GetViewport().SetInputAsHandled();
			return;
		}

		if (mouseButton.ButtonIndex != MouseButton.Left || mouseButton.Pressed)
		{
			return;
		}

		UpdateDropTargetFromMousePosition(mouseButton.GlobalPosition);
		CommitCurrentDrag();
	}

	public override void _Process(double delta)
	{
		if (_dragState != null)
		{
			UpdateDropTargetFromMousePosition(GetViewport().GetMousePosition());
		}

		if (Input.IsActionJustPressed(TakeAllAction))
		{
			TryTakeAllFromContainer();
		}

		if (Input.IsActionJustPressed(StashAllAction))
		{
			TryStashAllToContainer();
		}
	}

	public Vector2I GetSelectedCell(GridSide side)
	{
		return side switch
		{
			GridSide.Container => _containerSelectedCell,
			GridSide.Secure => _secureSelectedCell,
			_ => _playerSelectedCell,
		};
	}

	public bool IsWarehouseContainerActive()
	{
		return _uiController?.ActiveExternalContainerIsWarehouse ?? false;
	}

	public int GetSelectedWarehouseSaleCount()
	{
		PruneWarehouseSaleSelections();
		return _selectedWarehouseSaleItemIds.Count;
	}

	public int GetSelectedWarehouseSaleValue()
	{
		PruneWarehouseSaleSelections();
		GridContainerComponent? container = GetActiveContainerGrid();
		if (container == null)
		{
			return 0;
		}

		int total = 0;
		foreach (string instanceId in _selectedWarehouseSaleItemIds)
		{
			if (!container.ContainsItem(instanceId))
			{
				continue;
			}

			total += ItemValueClassifier.GetMarketValue(container.GetRequiredRecord(instanceId).Item);
		}

		return total;
	}

	public void ToggleWarehouseSellMode()
	{
		if (!IsWarehouseContainerActive())
		{
			return;
		}

		_warehouseSellModeActive = !_warehouseSellModeActive;
		if (!_warehouseSellModeActive)
		{
			_selectedWarehouseSaleItemIds.Clear();
		}

		_dragState = null;
		NotifyInteractionStateChanged();
	}

	public void SellSelectedWarehouseItems()
	{
		GridContainerComponent? container = GetActiveContainerGrid();
		if (!_warehouseSellModeActive || container == null || _selectedWarehouseSaleItemIds.Count == 0)
		{
			return;
		}

		System.Collections.Generic.List<string> soldInstanceIds = [];
		int creditsEarned = 0;
		foreach (string instanceId in _selectedWarehouseSaleItemIds)
		{
			if (!container.RemoveItem(instanceId, out ItemInstance? removedItem) || removedItem == null)
			{
				continue;
			}

			creditsEarned += ItemValueClassifier.GetMarketValue(removedItem);
			soldInstanceIds.Add(instanceId);
		}

		foreach (string instanceId in soldInstanceIds)
		{
			_selectedWarehouseSaleItemIds.Remove(instanceId);
		}

		if (creditsEarned <= 0)
		{
			NotifyInteractionStateChanged();
			return;
		}

		RunSession.Instance?.AddPlayerCredits(creditsEarned);
		EmitSignal(
			SignalName.StatusTextChanged,
			UiTextKeys.Inventory.StatusSoldItems,
			UiTextArgs.Create(
				("count", soldInstanceIds.Count),
				("value", creditsEarned)));
		NotifyInteractionStateChanged();
	}

	public bool TryResolveDetailRecord(DetailTarget target, out ContainerItemRecord? record)
	{
		record = null;
		if (target.Side == GridSide.Equipment)
		{
			ItemInstance? equippedItem = _equipmentComponent?.GetEquippedItem(target.EquipmentSlot);
			if (equippedItem == null)
			{
				return false;
			}

			record = new ContainerItemRecord
			{
				Item = equippedItem,
				Origin = Vector2I.Zero
			};
			return true;
		}

		if (string.IsNullOrWhiteSpace(target.InstanceId))
		{
			return false;
		}

		GridContainerComponent? container = GetContainerBySide(target.Side);
		if (container == null || !container.ContainsItem(target.InstanceId))
		{
			return false;
		}

		record = container.GetRequiredRecord(target.InstanceId);
		return true;
	}

	public void CloseDetails()
	{
		_detailTargets.Clear();
		NotifyInteractionStateChanged();
	}

	public bool TryCloseTopDetailTarget()
	{
		if (_detailTargets.Count == 0)
		{
			return false;
		}

		CloseDetailTarget(_detailTargets[^1]);
		return true;
	}

	public void CloseDetailTarget(DetailTarget target)
	{
		_detailTargets.Remove(target);

		NotifyInteractionStateChanged();
	}

	public void HandleGridCellPressed(GridSide side, Vector2I cell, int buttonIndex)
	{
		if (!IsPlayerWindowOpen())
		{
			return;
		}

		_focusedSide = side;
		SetSelectedCell(side, cell);
		GridContainerComponent? sourceContainer = GetContainerBySide(side);
		ContainerItemRecord? record = sourceContainer?.FindItemAt(cell);

		if (side == GridSide.Container && _warehouseSellModeActive)
		{
			_dragState = null;
			if (buttonIndex == (int)MouseButton.Left)
			{
				ToggleWarehouseSaleSelection(record);
			}

			NotifyInteractionStateChanged();
			return;
		}

		if (buttonIndex == (int)MouseButton.Right && _dragState == null)
		{
			if (side == GridSide.Player && sourceContainer != null && record != null)
			{
				TryUsePlayerInventoryItem(sourceContainer, record);
				NotifyInteractionStateChanged();
			}

			return;
		}

		if (buttonIndex == (int)MouseButton.Left
			&& (Input.IsKeyPressed(Key.Ctrl) || Input.IsKeyPressed(Key.Meta))
			&& _dragState == null
			&& sourceContainer != null
			&& record != null)
		{
			if (!CanInteractWithContainerRecord(sourceContainer, record))
			{
				EmitUnsearchedInteractionBlocked();
				return;
			}

			TryQuickTransferRecord(side, sourceContainer, record);
			return;
		}

		if (buttonIndex == (int)MouseButton.Right && _dragState != null)
		{
			_dragState.PreviewRotated = !_dragState.PreviewRotated;
			NotifyInteractionStateChanged();
			return;
		}

		if (buttonIndex != (int)MouseButton.Left)
		{
			return;
		}

		if (record == null || sourceContainer == null)
		{
			_dragState = null;
			CloseDetails();
			NotifyInteractionStateChanged();
			return;
		}

		if (!CanInteractWithContainerRecord(sourceContainer, record))
		{
			_dragState = null;
			NotifyInteractionStateChanged();
			return;
		}

		_dragState = new DragState
		{
			SourceSide = side,
			SourceContainer = sourceContainer,
			InstanceId = record.Item.InstanceId,
			SourceOrigin = record.Origin,
			PreviewRotated = record.Item.IsRotated,
			HoverCell = cell,
			HoverSide = side,
		};
		NotifyInteractionStateChanged();
	}

	public void HandleGridCellReleased(GridSide side, Vector2I cell, int buttonIndex)
	{
		if (buttonIndex != (int)MouseButton.Left || _dragState == null)
		{
			return;
		}

		_dragState.HoverSide = side;
		_dragState.HoverCell = cell;
		NotifyInteractionStateChanged();
	}

	public void HandleGridCellDoubleClicked(GridSide side, Vector2I cell, int buttonIndex)
	{
		if (buttonIndex != (int)MouseButton.Left || !IsPlayerWindowOpen())
		{
			return;
		}

		if (side == GridSide.Container && _warehouseSellModeActive)
		{
			return;
		}

		_focusedSide = side;
		SetSelectedCell(side, cell);
		_dragState = null;

		GridContainerComponent? sourceContainer = GetContainerBySide(side);
		ContainerItemRecord? record = sourceContainer?.FindItemAt(cell);
		if (record == null)
		{
			return;
		}
		if (!CanInteractWithContainerRecord(sourceContainer, record))
		{
			return;
		}

		OpenDetailTarget(new DetailTarget(side, record.Item.InstanceId, EquipmentSlotType.None));
		NotifyInteractionStateChanged();
	}

	public void HandleGridCellHovered(GridSide side, Vector2I cell)
	{
		_focusedSide = side;
		SetSelectedCell(side, cell);
		if (_dragState != null)
		{
			_dragState.HoverSide = side;
			_dragState.HoverCell = cell;
			_dragState.HoverEquipmentSlot = EquipmentSlotType.None;
			NotifyInteractionStateChanged();
		}
	}

	public void HandleGridPointerExited(GridSide side)
	{
		if (_dragState != null && _dragState.HoverSide == side)
		{
			_dragState.HoverSide = GridSide.None;
			_dragState.HoverEquipmentSlot = EquipmentSlotType.None;
			NotifyInteractionStateChanged();
		}
	}

	public bool TryGetDragHint(out string text, out Vector2 position)
	{
		text = string.Empty;
		position = Vector2.Zero;
		if (_dragState == null)
		{
			return false;
		}

		ItemInstance? item = ResolveDragItem(_dragState);
		if (item == null)
		{
			return false;
		}

		text = UiText.Resolve(
			UiTextKeys.Inventory.DragHint,
			("name", item.Definition.DisplayName),
			("count", item.StackCount));
		position = GetViewport().GetMousePosition() + new Vector2(18.0f, 18.0f);
		return true;
	}

	public void BuildPreviews(
		out Rect2I? containerPreview,
		out bool containerPreviewValid,
		out Rect2I? securePreview,
		out bool securePreviewValid,
		out Rect2I? playerPreview,
		out bool playerPreviewValid)
	{
		containerPreview = null;
		containerPreviewValid = true;
		securePreview = null;
		securePreviewValid = true;
		playerPreview = null;
		playerPreviewValid = true;

		if (_dragState == null || _dragState.SourceSide == GridSide.Equipment)
		{
			return;
		}

		if (_dragState.SourceContainer == null || !_dragState.SourceContainer.ContainsItem(_dragState.InstanceId))
		{
			return;
		}

		ContainerItemRecord record = _dragState.SourceContainer.GetRequiredRecord(_dragState.InstanceId);
		ItemInstance previewItem = record.Item.CreateCopy();
		if (previewItem.IsRotated != _dragState.PreviewRotated)
		{
			previewItem.TrySetRotation(_dragState.PreviewRotated);
		}

		GridContainerComponent? targetContainer = GetContainerBySide(_dragState.HoverSide);
		if (targetContainer == null)
		{
			return;
		}

		Rect2I previewRect = new(_dragState.HoverCell, previewItem.GetFootprint());
		bool previewValid = targetContainer == _dragState.SourceContainer
			? targetContainer.CanPlaceItem(previewItem, _dragState.HoverCell, _dragState.InstanceId)
				|| targetContainer.GetMergeAmountAt(_dragState.HoverCell, record.Item, _dragState.InstanceId) > 0
			: targetContainer.CanPlaceItem(previewItem, _dragState.HoverCell)
				|| targetContainer.GetMergeAmountAt(_dragState.HoverCell, record.Item) > 0;

		if (_dragState.HoverSide == GridSide.Container)
		{
			containerPreview = previewRect;
			containerPreviewValid = previewValid;
		}
		else if (_dragState.HoverSide == GridSide.Player)
		{
			playerPreview = previewRect;
			playerPreviewValid = previewValid;
		}
		else if (_dragState.HoverSide == GridSide.Secure)
		{
			securePreview = previewRect;
			securePreviewValid = previewValid;
		}
	}

	private void OnEquipmentSlotPressed(int slotTypeValue, int buttonIndex)
	{
		if (buttonIndex != (int)MouseButton.Left || !IsPlayerWindowOpen())
		{
			return;
		}

		EquipmentSlotType slotType = (EquipmentSlotType)slotTypeValue;
		_focusedSide = GridSide.Equipment;
		ItemInstance? equippedItem = _equipmentComponent?.GetEquippedItem(slotType);
		if (equippedItem == null)
		{
			_dragState = null;
			NotifyInteractionStateChanged();
			return;
		}

		_dragState = new DragState
		{
			SourceSide = GridSide.Equipment,
			SourceEquipmentSlot = slotType,
			InstanceId = equippedItem.InstanceId,
			PreviewRotated = equippedItem.IsRotated,
			HoverSide = GridSide.Equipment,
			HoverEquipmentSlot = slotType,
		};
		NotifyInteractionStateChanged();
	}

	private void OnEquipmentSlotDoublePressed(int slotTypeValue)
	{
		if (!IsPlayerWindowOpen())
		{
			return;
		}

		EquipmentSlotType slotType = (EquipmentSlotType)slotTypeValue;
		ItemInstance? equippedItem = _equipmentComponent?.GetEquippedItem(slotType);
		if (equippedItem == null)
		{
			return;
		}

		OpenDetailTarget(new DetailTarget(GridSide.Equipment, equippedItem.InstanceId, slotType));
		NotifyInteractionStateChanged();
	}

	private void OnUiStateChanged()
	{
		GridContainerComponent? activeContainer = GetActiveContainerGrid();
		if (_lastKnownContainer != activeContainer)
		{
			_dragState = null;
			_warehouseSellModeActive = false;
			_selectedWarehouseSaleItemIds.Clear();
			CloseDetails();
			InitializeSelections();
			_lastKnownContainer = activeContainer;
			NotifyInteractionStateChanged();
		}
		else if (!IsPlayerWindowOpen())
		{
			_dragState = null;
			_focusedSide = GridSide.None;
			_warehouseSellModeActive = false;
			_selectedWarehouseSaleItemIds.Clear();
			CloseDetails();
			NotifyInteractionStateChanged();
		}
	}

	private void TryTakeAllFromContainer()
	{
		GridContainerComponent? sourceContainer = GetActiveContainerGrid();
		if (sourceContainer == null || _playerInventory == null)
		{
			return;
		}

		System.Collections.Generic.List<string> instanceIds = [];
		int blockedItemCount = 0;
		foreach (ContainerItemRecord record in sourceContainer.ItemRecords)
		{
			if (!CanInteractWithContainerRecord(sourceContainer, record))
			{
				blockedItemCount++;
				continue;
			}

			instanceIds.Add(record.Item.InstanceId);
		}

		int movedItemCount = 0;
		int failedItemCount = blockedItemCount;
		foreach (string instanceId in instanceIds)
		{
			if (InventoryTransferService.TryQuickTransfer(sourceContainer, _playerInventory.GetPrimaryContainer(), instanceId))
			{
				movedItemCount++;
			}
			else
			{
				failedItemCount++;
			}
		}

		EmitSignal(
			SignalName.StatusTextChanged,
			UiTextKeys.Inventory.StatusTakeAll,
			UiTextArgs.Create(
				("moved", movedItemCount),
				("failed", failedItemCount)));
	}

	private void TryStashAllToContainer()
	{
		GridContainerComponent? playerContainer = _playerInventory?.GetPrimaryContainer();
		GridContainerComponent? targetContainer = GetActiveContainerGrid();
		if (playerContainer == null || targetContainer == null)
		{
			return;
		}

		System.Collections.Generic.List<string> instanceIds = [];
		foreach (ContainerItemRecord record in playerContainer.ItemRecords)
		{
			instanceIds.Add(record.Item.InstanceId);
		}

		int movedItemCount = 0;
		int failedItemCount = 0;
		foreach (string instanceId in instanceIds)
		{
			if (InventoryTransferService.TryQuickTransfer(playerContainer, targetContainer, instanceId))
			{
				movedItemCount++;
			}
			else
			{
				failedItemCount++;
			}
		}

		EmitSignal(
			SignalName.StatusTextChanged,
			UiTextKeys.Inventory.StatusTakeAll,
			UiTextArgs.Create(
				("moved", movedItemCount),
				("failed", failedItemCount)));
		NotifyInteractionStateChanged();
	}

	private void CommitCurrentDrag()
	{
		if (_dragState == null)
		{
			return;
		}

		_customFailureStatusEmitted = false;

		bool moved = _dragState.SourceSide switch
		{
			GridSide.Player or GridSide.Container or GridSide.Secure when _dragState.HoverWeaponSkillSlotIndex >= 0 => TryInstallDraggedWeaponSkill(_dragState),
			GridSide.Player or GridSide.Container when _dragState.HoverSide == GridSide.Equipment => TryEquipDraggedItem(_dragState),
			GridSide.Equipment when _dragState.HoverSide == GridSide.Equipment => TryRearrangeEquippedItem(_dragState),
			GridSide.Equipment when _dragState.HoverSide == GridSide.Player || _dragState.HoverSide == GridSide.Container => TryUnequipDraggedItem(_dragState),
			GridSide.Player or GridSide.Container or GridSide.Secure => TryMoveDraggedContainerItem(_dragState),
			_ => false
		};

		if (!moved
			&& !_customFailureStatusEmitted
			&& _dragState.SourceSide != GridSide.Equipment
			&& _dragState.HoverSide != GridSide.Equipment)
		{
			ItemInstance? sourceItem = ResolveDragItem(_dragState);
			if (sourceItem != null)
			{
				EmitSignal(
					SignalName.StatusTextChanged,
					UiTextKeys.Inventory.StatusMoveFailure,
					UiTextArgs.Create(("name", sourceItem.Definition.DisplayName)));
			}
		}

		_dragState = null;
		NotifyInteractionStateChanged();
	}

	private bool TryMoveDraggedContainerItem(DragState dragState)
	{
		if (dragState.SourceContainer == null || !dragState.SourceContainer.ContainsItem(dragState.InstanceId))
		{
			return false;
		}

		ContainerItemRecord sourceRecord = dragState.SourceContainer.GetRequiredRecord(dragState.InstanceId);
		if (!CanInteractWithContainerRecord(dragState.SourceContainer, sourceRecord))
		{
			EmitUnsearchedInteractionBlocked();
			return false;
		}

		GridContainerComponent? targetContainer = GetContainerBySide(dragState.HoverSide);
		if (dragState.SourceSide == GridSide.Secure
			&& targetContainer != null
			&& targetContainer != dragState.SourceContainer
			&& !IsSecureContainerTarget(targetContainer)
			&& !TrySettleSecureContainerBillForTransfer(sourceRecord.Item))
		{
			return false;
		}

		bool moved = false;
		if (targetContainer != null)
		{
			if (targetContainer == dragState.SourceContainer)
			{
				moved = InventoryTransferService.TryRepositionWithinContainer(
					targetContainer,
					dragState.InstanceId,
					dragState.HoverCell,
					dragState.PreviewRotated);
			}
			else if (IsSecureContainerTarget(targetContainer))
			{
				moved = TryTransferIntoSecureContainer(
					dragState.SourceContainer,
					targetContainer,
					dragState.InstanceId,
					dragState.HoverCell,
					dragState.PreviewRotated,
					sourceRecord.Item,
					out SecureContainerStoreFailureReason secureFailureReason);
				if (!moved && secureFailureReason != SecureContainerStoreFailureReason.None)
				{
					EmitSecureStoreFailure(sourceRecord.Item, secureFailureReason);
				}
			}
			else
			{
				moved = InventoryTransferService.TryTransferToPosition(
					dragState.SourceContainer,
					targetContainer,
					dragState.InstanceId,
					dragState.HoverCell,
					dragState.PreviewRotated);
			}
		}

		if (moved)
		{
			EmitSignal(
				SignalName.StatusTextChanged,
				UiTextKeys.Inventory.StatusMoveSuccess,
				UiTextArgs.Create(("name", sourceRecord.Item.Definition.DisplayName)));
		}

		return moved;
	}

	private bool TryInstallDraggedWeaponSkill(DragState dragState)
	{
		if (dragState.SourceContainer == null
			|| !dragState.SourceContainer.ContainsItem(dragState.InstanceId)
			|| dragState.HoverDetailTarget == null
			|| dragState.HoverWeaponSkillSlotIndex < 0)
		{
			EmitSignal(
				SignalName.StatusTextChanged,
				UiTextKeys.Inventory.StatusWeaponSkillEquipFailed,
				UiTextArgs.Create(("slot", dragState.HoverWeaponSkillSlotIndex + 1)));
			return false;
		}

		ContainerItemRecord sourceRecord = dragState.SourceContainer.GetRequiredRecord(dragState.InstanceId);
		WeaponSkillItemDefinition? weaponSkillItem = sourceRecord.Item.Definition.WeaponSkill;
		DetailTarget detailTarget = dragState.HoverDetailTarget.Value;
		if (weaponSkillItem == null)
		{
			EmitSignal(
				SignalName.StatusTextChanged,
				UiTextKeys.Inventory.StatusWeaponSkillEquipFailed,
				UiTextArgs.Create(("slot", dragState.HoverWeaponSkillSlotIndex + 1)));
			return false;
		}

		bool installed = detailTarget.Side switch
		{
			GridSide.Equipment => TryInstallWeaponSkillOnEquippedWeaponTarget(
				detailTarget,
				weaponSkillItem,
				sourceRecord.Item.Definition,
				sourceRecord.Item.AcquisitionState,
				sourceRecord.Item.RemainingUses,
				dragState.HoverWeaponSkillSlotIndex,
				dragState.SourceContainer),
			GridSide.Player or GridSide.Container or GridSide.Secure => TryInstallWeaponSkillOnStoredWeapon(
				detailTarget,
				weaponSkillItem,
				sourceRecord.Item.Definition,
				sourceRecord.Item.AcquisitionState,
				sourceRecord.Item.RemainingUses,
				dragState.HoverWeaponSkillSlotIndex,
				dragState.SourceContainer),
			_ => false,
		};

		if (!installed)
		{
			EmitSignal(
				SignalName.StatusTextChanged,
				UiTextKeys.Inventory.StatusWeaponSkillEquipFailed,
				UiTextArgs.Create(("slot", dragState.HoverWeaponSkillSlotIndex + 1)));
			return false;
		}

		if (sourceRecord.Item.HasLimitedUses)
		{
			dragState.SourceContainer.TryConsumeItemUse(dragState.InstanceId);
		}
		else if (weaponSkillItem.ConsumeOnInstall)
		{
			dragState.SourceContainer.TryConsumeItemStack(dragState.InstanceId, 1);
		}

		EmitSignal(
			SignalName.StatusTextChanged,
			UiTextKeys.Inventory.StatusWeaponSkillEquipped,
			UiTextArgs.Create(
				("name", sourceRecord.Item.Definition.DisplayName),
				("slot", dragState.HoverWeaponSkillSlotIndex + 1)));
		return true;
	}

	private bool TryInstallWeaponSkillOnEquippedWeaponTarget(
		DetailTarget detailTarget,
		WeaponSkillItemDefinition weaponSkillItem,
		ItemDefinition sourceItemDefinition,
		ItemAcquisitionState sourceAcquisitionState,
		int sourceRemainingUses,
		int slotIndex,
		GridContainerComponent sourceContainer)
	{
		if (_equipmentComponent?.TryInstallWeaponSkillOnEquippedWeaponSlot(
				detailTarget.EquipmentSlot,
				weaponSkillItem,
				sourceItemDefinition,
				sourceAcquisitionState,
				sourceRemainingUses,
				slotIndex,
				replaceExisting: true,
				out InstalledWeaponSkillState? replacedState) != true)
		{
			return false;
		}

		if (TryReturnWeaponSkillState(replacedState, sourceContainer))
		{
			return true;
		}

		if (replacedState != null)
		{
			_equipmentComponent.TryRestoreEquippedWeaponSkillState(detailTarget.EquipmentSlot, replacedState, replaceExisting: true);
		}

		return false;
	}

	private bool TryInstallWeaponSkillOnStoredWeapon(
		DetailTarget detailTarget,
		WeaponSkillItemDefinition weaponSkillItem,
		ItemDefinition sourceItemDefinition,
		ItemAcquisitionState sourceAcquisitionState,
		int sourceRemainingUses,
		int slotIndex,
		GridContainerComponent sourceContainer)
	{
		if (!TryResolveDetailRecord(detailTarget, out ContainerItemRecord? targetRecord)
			|| targetRecord?.Item?.Definition.Equipment is not WeaponEquipmentDefinition weapon
			|| !weaponSkillItem.CanInstallOnWeapon(weapon))
		{
			return false;
		}

		if (!targetRecord.Item.TryInstallWeaponSkillAtSlot(
			weaponSkillItem,
			sourceRemainingUses,
			sourceItemDefinition,
			sourceAcquisitionState,
			slotIndex,
			replaceExisting: true,
			out InstalledWeaponSkillState? replacedState))
		{
			return false;
		}

		if (TryReturnWeaponSkillState(replacedState, sourceContainer))
		{
			return true;
		}

		if (replacedState != null)
		{
			targetRecord.Item.TryRestoreWeaponSkillState(replacedState, replaceExisting: true);
		}

		return false;
	}

	public void HandleWeaponSkillSlotPressed(DetailTarget detailTarget, int slotIndex, int buttonIndex)
	{
		if (!IsPlayerWindowOpen() || buttonIndex != (int)MouseButton.Right)
		{
			return;
		}

		if (!TryClearWeaponSkillSlot(detailTarget, slotIndex, out InstalledWeaponSkillState? removedState))
		{
			return;
		}

		if (!TryReturnWeaponSkillState(removedState, ResolvePreferredRefundContainer(detailTarget)))
		{
			TryRestoreWeaponSkillSlot(detailTarget, removedState);
			EmitSignal(
				SignalName.StatusTextChanged,
				UiTextKeys.Inventory.StatusWeaponSkillEquipFailed,
				UiTextArgs.Create(("slot", slotIndex + 1)));
			return;
		}

		EmitSignal(
			SignalName.StatusTextChanged,
			UiTextKeys.Inventory.StatusWeaponSkillRemoved,
			UiTextArgs.Create(
				("name", removedState?.Skill?.DisplayName ?? "-"),
				("slot", slotIndex + 1)));
		NotifyInteractionStateChanged();
	}

	private bool TryClearWeaponSkillSlot(DetailTarget detailTarget, int slotIndex, out InstalledWeaponSkillState? removedState)
	{
		removedState = null;
		return detailTarget.Side switch
		{
			GridSide.Equipment => _equipmentComponent?.TryClearEquippedWeaponSkillSlot(detailTarget.EquipmentSlot, slotIndex, out removedState) == true,
			GridSide.Player or GridSide.Container or GridSide.Secure => TryResolveDetailRecord(detailTarget, out ContainerItemRecord? record)
				&& record?.Item.TryClearWeaponSkillSlot(slotIndex, out removedState) == true,
			_ => false,
		};
	}

	private bool TryRestoreWeaponSkillSlot(DetailTarget detailTarget, InstalledWeaponSkillState? state)
	{
		if (state == null)
		{
			return false;
		}

		return detailTarget.Side switch
		{
			GridSide.Equipment => _equipmentComponent?.TryRestoreEquippedWeaponSkillState(detailTarget.EquipmentSlot, state, replaceExisting: true) == true,
			GridSide.Player or GridSide.Container or GridSide.Secure => TryResolveDetailRecord(detailTarget, out ContainerItemRecord? record)
				&& record?.Item.TryRestoreWeaponSkillState(state, replaceExisting: true) == true,
			_ => false,
		};
	}

	private bool TryReturnWeaponSkillState(InstalledWeaponSkillState? state, GridContainerComponent? preferredContainer)
	{
		ItemInstance? refundItem = state?.CreateRefundItem();
		if (refundItem == null)
		{
			return true;
		}

		if (preferredContainer != null && TryAddItemAnywhereWithSecureRules(preferredContainer, refundItem))
		{
			return true;
		}

		GridContainerComponent? playerContainer = _playerInventory?.GetPrimaryContainer();
		if (playerContainer != null && playerContainer != preferredContainer && TryAddItemAnywhereWithSecureRules(playerContainer, refundItem))
		{
			return true;
		}

		GridContainerComponent? secureContainer = GetSecureContainerGrid();
		return secureContainer != null
			&& secureContainer != preferredContainer
			&& secureContainer != playerContainer
			&& TryAddItemAnywhereWithSecureRules(secureContainer, refundItem);
	}

	private GridContainerComponent? ResolvePreferredRefundContainer(DetailTarget detailTarget)
	{
		return detailTarget.Side switch
		{
			GridSide.Player => GetContainerBySide(GridSide.Player),
			GridSide.Container => GetContainerBySide(GridSide.Container) ?? GetContainerBySide(GridSide.Player),
			GridSide.Secure => GetContainerBySide(GridSide.Secure) ?? GetContainerBySide(GridSide.Player),
			GridSide.Equipment => GetContainerBySide(GridSide.Player) ?? GetContainerBySide(GridSide.Secure),
			_ => GetContainerBySide(GridSide.Player),
		};
	}

	private bool TryEquipDraggedItem(DragState dragState)
	{
		if (dragState.SourceContainer == null || !dragState.SourceContainer.RemoveItem(dragState.InstanceId, out ItemInstance? removedItem) || removedItem == null)
		{
			EmitEquipFailure(null, dragState.HoverEquipmentSlot, EquipmentActionFailureReason.MissingEquipmentDefinition);
			return false;
		}

		if (_equipmentComponent == null)
		{
			dragState.SourceContainer.TryPlaceIncomingItemAt(removedItem, dragState.SourceOrigin, dragState.PreviewRotated);
			EmitEquipFailure(removedItem, dragState.HoverEquipmentSlot, EquipmentActionFailureReason.MissingSlot);
			return false;
		}

		if (!_equipmentComponent.TryEquip(dragState.HoverEquipmentSlot, removedItem, out _, out EquipmentActionFailureReason failureReason))
		{
			dragState.SourceContainer.TryPlaceIncomingItemAt(removedItem, dragState.SourceOrigin, dragState.PreviewRotated);
			EmitEquipFailure(removedItem, dragState.HoverEquipmentSlot, failureReason);
			return false;
		}

		EmitSignal(
			SignalName.StatusTextChanged,
			UiTextKeys.Inventory.StatusEquipSuccess,
			UiTextArgs.Create(
				("name", removedItem.Definition.DisplayName),
				("slot", EquipmentTextFormatter.GetSlotDisplayName(dragState.HoverEquipmentSlot))));
		return true;
	}

	private bool TryUnequipDraggedItem(DragState dragState)
	{
		EquipmentActionFailureReason failureReason = EquipmentActionFailureReason.None;
		ItemInstance? removedItem = null;
		if (_equipmentComponent == null
			|| !_equipmentComponent.TryUnequip(dragState.SourceEquipmentSlot, out removedItem, out failureReason)
			|| removedItem == null)
		{
			EmitUnequipFailure(null, dragState.SourceEquipmentSlot, _equipmentComponent == null ? EquipmentActionFailureReason.MissingSlot : failureReason);
			return false;
		}

		GridContainerComponent? targetContainer = GetContainerBySide(dragState.HoverSide);
		if (targetContainer == null || !TryPlaceEquippedItemIntoContainer(targetContainer, dragState.HoverCell, removedItem))
		{
			_equipmentComponent.TryEquip(dragState.SourceEquipmentSlot, removedItem, out _, out _);
			EmitUnequipFailure(removedItem, dragState.SourceEquipmentSlot, EquipmentActionFailureReason.TargetContainerRejected);
			return false;
		}

		EmitSignal(
			SignalName.StatusTextChanged,
			UiTextKeys.Inventory.StatusUnequipSuccess,
			UiTextArgs.Create(
				("name", removedItem.Definition.DisplayName),
				("slot", EquipmentTextFormatter.GetSlotDisplayName(dragState.SourceEquipmentSlot))));
		return true;
	}

	private bool TryRearrangeEquippedItem(DragState dragState)
	{
		ItemInstance? sourceItem = _equipmentComponent?.GetEquippedItem(dragState.SourceEquipmentSlot);
		if (_equipmentComponent == null || sourceItem == null)
		{
			EmitEquipFailure(null, dragState.HoverEquipmentSlot, EquipmentActionFailureReason.MissingSlot);
			return false;
		}

		if (!_equipmentComponent.TryRearrangeEquippedItem(
			dragState.SourceEquipmentSlot,
			dragState.HoverEquipmentSlot,
			out EquipmentActionFailureReason failureReason))
		{
			EmitEquipFailure(sourceItem, dragState.HoverEquipmentSlot, failureReason);
			return false;
		}

		return true;
	}

	private bool TryPlaceEquippedItemIntoContainer(GridContainerComponent targetContainer, Vector2I cell, ItemInstance item)
	{
		if (IsSecureContainerTarget(targetContainer))
		{
			RunSession? runSession = RunSession.Instance;
			string secureContainerId = ResolveSecureContainerId(targetContainer);
			SecureContainerStoreFailureReason secureFailureReason = SecureContainerStoreFailureReason.None;
			if (runSession == null || !runSession.CanStoreInSecureContainer(secureContainerId, item, out secureFailureReason))
			{
				EmitSecureStoreFailure(item, secureFailureReason);
				return false;
			}
		}

		if (targetContainer.TryPlaceIncomingItemAt(item, cell, item.IsRotated))
		{
			if (IsSecureContainerTarget(targetContainer))
			{
				RunSession.Instance?.NotifySecureContainerValueStored(ResolveSecureContainerId(targetContainer), item.InstanceId, item.Definition.DisplayName, ItemValueClassifier.GetMarketValue(item));
			}
			return true;
		}

		if (targetContainer.TryMergeIncomingItemAt(cell, item, out int mergedAmount) && mergedAmount >= item.StackCount)
		{
			if (IsSecureContainerTarget(targetContainer))
			{
				RunSession.Instance?.NotifySecureContainerValueStored(ResolveSecureContainerId(targetContainer), item.InstanceId, item.Definition.DisplayName, ItemValueClassifier.GetMarketValue(item));
			}
			return true;
		}

		return false;
	}

	private void UpdateDropTargetFromMousePosition(Vector2 globalMousePosition)
	{
		if (_dragState == null)
		{
			return;
		}

		if (_itemScreenCoordinator != null
			&& _itemScreenCoordinator.TryGetWeaponSkillSlotTargetFromGlobalPosition(globalMousePosition, out DetailTarget detailTarget, out int slotIndex))
		{
			_dragState.HoverSide = detailTarget.Side;
			_dragState.HoverEquipmentSlot = detailTarget.EquipmentSlot;
			_dragState.HoverDetailTarget = detailTarget;
			_dragState.HoverWeaponSkillSlotIndex = slotIndex;
			return;
		}

		if (_equipmentPanelPresenter != null
			&& _equipmentPanelPresenter.TryGetSlotFromGlobalPosition(globalMousePosition, out EquipmentSlotType equipmentSlot))
		{
			_dragState.HoverSide = GridSide.Equipment;
			_dragState.HoverEquipmentSlot = equipmentSlot;
			_dragState.HoverDetailTarget = null;
			_dragState.HoverWeaponSkillSlotIndex = -1;
			return;
		}

		if (_playerGridView != null && IsPlayerWindowOpen()
			&& _playerGridView.TryGetCellFromGlobalPosition(globalMousePosition, out Vector2I playerCell))
		{
			_dragState.HoverSide = GridSide.Player;
			_dragState.HoverCell = playerCell;
			_dragState.HoverEquipmentSlot = EquipmentSlotType.None;
			_dragState.HoverDetailTarget = null;
			_dragState.HoverWeaponSkillSlotIndex = -1;
			return;
		}

		if (_containerGridView != null
			&& GetActiveContainerGrid() != null
			&& _containerGridView.TryGetCellFromGlobalPosition(globalMousePosition, out Vector2I containerCell))
		{
			_dragState.HoverSide = GridSide.Container;
			_dragState.HoverCell = containerCell;
			_dragState.HoverEquipmentSlot = EquipmentSlotType.None;
			_dragState.HoverDetailTarget = null;
			_dragState.HoverWeaponSkillSlotIndex = -1;
			return;
		}

		if (_secureGridView != null
			&& GetSecureContainerGrid() != null
			&& _secureGridView.TryGetCellFromGlobalPosition(globalMousePosition, out Vector2I secureCell))
		{
			_dragState.HoverSide = GridSide.Secure;
			_dragState.HoverCell = secureCell;
			_dragState.HoverEquipmentSlot = EquipmentSlotType.None;
			_dragState.HoverDetailTarget = null;
			_dragState.HoverWeaponSkillSlotIndex = -1;
			return;
		}

		_dragState.HoverSide = GridSide.None;
		_dragState.HoverEquipmentSlot = EquipmentSlotType.None;
		_dragState.HoverDetailTarget = null;
		_dragState.HoverWeaponSkillSlotIndex = -1;
	}

	private void InitializeSelections()
	{
		_playerSelectedCell = Vector2I.Zero;
		_containerSelectedCell = Vector2I.Zero;
		_secureSelectedCell = Vector2I.Zero;
		_focusedSide = GetActiveContainerGrid() == null ? GridSide.Player : GridSide.Container;

		GridContainerComponent? containerGrid = GetActiveContainerGrid();
		if (containerGrid != null)
		{
			foreach (ContainerItemRecord record in containerGrid.ItemRecords)
			{
				_containerSelectedCell = record.Origin;
				break;
			}
		}

		GridContainerComponent? secureGrid = GetSecureContainerGrid();
		if (secureGrid != null)
		{
			foreach (ContainerItemRecord record in secureGrid.ItemRecords)
			{
				_secureSelectedCell = record.Origin;
				break;
			}
		}
	}

	private void SetSelectedCell(GridSide side, Vector2I cell)
	{
		if (side == GridSide.Player)
		{
			_playerSelectedCell = cell;
			return;
		}

		if (side == GridSide.Container)
		{
			_containerSelectedCell = cell;
			return;
		}

		if (side == GridSide.Secure)
		{
			_secureSelectedCell = cell;
		}
	}

	private ItemInstance? ResolveDragItem(DragState dragState)
	{
		return dragState.SourceSide switch
		{
			GridSide.Equipment => _equipmentComponent?.GetEquippedItem(dragState.SourceEquipmentSlot),
			_ when dragState.SourceContainer != null && dragState.SourceContainer.ContainsItem(dragState.InstanceId)
				=> dragState.SourceContainer.GetRequiredRecord(dragState.InstanceId).Item,
			_ => null
		};
	}

	private GridContainerComponent? GetContainerBySide(GridSide side)
	{
		return side switch
		{
			GridSide.Player => _playerInventory?.GetPrimaryContainer(),
			GridSide.Container => GetActiveContainerGrid(),
			GridSide.Secure => GetSecureContainerGrid(),
			_ => null
		};
	}

	private bool IsPlayerWindowOpen()
	{
		return _uiController?.PlayerWindowOpen ?? false;
	}

	private void OpenDetailTarget(DetailTarget target)
	{
		if (_detailTargets.Contains(target))
		{
			return;
		}

		_detailTargets.Add(target);
	}

	private void TryQuickTransferRecord(GridSide side, GridContainerComponent sourceContainer, ContainerItemRecord record)
	{
		if (!CanInteractWithContainerRecord(sourceContainer, record))
		{
			EmitUnsearchedInteractionBlocked();
			return;
		}

		GridSide targetSide = side switch
		{
			GridSide.Player => GetActiveContainerGrid() != null ? GridSide.Container : GridSide.Secure,
			GridSide.Container => GridSide.Player,
			GridSide.Secure => GetActiveContainerGrid() != null ? GridSide.Container : GridSide.Player,
			_ => GridSide.None,
		};

		GridContainerComponent? targetContainer = GetContainerBySide(targetSide);
		if (targetContainer == null)
		{
			return;
		}

		string itemName = record.Item.Definition.DisplayName;
		if (side == GridSide.Secure && !TrySettleSecureContainerBillForTransfer(record.Item))
		{
			return;
		}

		bool moved;
		if (IsSecureContainerTarget(targetContainer))
		{
			moved = TryTransferIntoSecureContainer(
				sourceContainer,
				targetContainer,
				record.Item.InstanceId,
				null,
				record.Item.IsRotated,
				record.Item,
				out SecureContainerStoreFailureReason secureFailureReason);
			if (!moved && secureFailureReason != SecureContainerStoreFailureReason.None)
			{
				EmitSecureStoreFailure(record.Item, secureFailureReason);
				return;
			}
		}
		else
		{
			moved = InventoryTransferService.TryQuickTransfer(sourceContainer, targetContainer, record.Item.InstanceId);
		}

		if (!moved)
		{
			EmitSignal(
				SignalName.StatusTextChanged,
				UiTextKeys.Inventory.StatusQuickTransferFailure,
				UiTextArgs.Create(("name", itemName)));
			return;
		}

		EmitSignal(
			SignalName.StatusTextChanged,
			UiTextKeys.Inventory.StatusQuickTransferSuccess,
			UiTextArgs.Create(("name", itemName)));
		NotifyInteractionStateChanged();
	}

	private bool IsSecureContainerTarget(GridContainerComponent? targetContainer)
	{
		GridContainerComponent? secureContainer = GetSecureContainerGrid();
		return targetContainer != null && targetContainer == secureContainer;
	}

	private static bool CanInteractWithContainerRecord(GridContainerComponent? container, ContainerItemRecord? record)
	{
		return container == null
			|| record == null
			|| container.SearchRuntime?.CanInteractWithItem(record.Item.InstanceId) != false;
	}

	private void EmitUnsearchedInteractionBlocked()
	{
		bool chinese = UiText.CurrentLocale == UiText.DefaultLocale;
		EmitSignal(
			SignalName.StatusTextChanged,
			UiTextKeys.Inventory.StatusMoveFailure,
			UiTextArgs.Create(("name", chinese ? "未搜索物品" : "Unsearched Item")));
	}

	private bool TryTransferIntoSecureContainer(
		GridContainerComponent sourceContainer,
		GridContainerComponent secureContainer,
		string instanceId,
		Vector2I? targetCell,
		bool rotated,
		ItemInstance sourceItem,
		out SecureContainerStoreFailureReason secureFailureReason)
	{
		secureFailureReason = SecureContainerStoreFailureReason.None;
		RunSession? runSession = RunSession.Instance;
		string secureContainerId = ResolveSecureContainerId(secureContainer);
		if (runSession == null || !runSession.CanStoreInSecureContainer(secureContainerId, sourceItem, out secureFailureReason))
		{
			return false;
		}

		int transferredValue = ItemValueClassifier.GetMarketValue(sourceItem);
		if (targetCell.HasValue)
		{
			ItemInstance previewItem = sourceItem.CreateCopy();
			previewItem.TrySetRotation(rotated);
			if (!secureContainer.CanPlaceItem(previewItem, targetCell.Value))
			{
				int mergeAmount = secureContainer.GetMergeAmountAt(targetCell.Value, sourceItem);
				if (mergeAmount > 0)
				{
					transferredValue = Mathf.Max(0, sourceItem.Definition.BaseValue) * mergeAmount;
				}
			}
		}

		bool moved = targetCell.HasValue
			? InventoryTransferService.TryTransferToPosition(sourceContainer, secureContainer, instanceId, targetCell.Value, rotated)
			: InventoryTransferService.TryQuickTransfer(sourceContainer, secureContainer, instanceId);

		if (moved)
		{
			runSession.NotifySecureContainerValueStored(secureContainerId, instanceId, sourceItem.Definition.DisplayName, transferredValue);
		}

		return moved;
	}

	private bool TryAddItemAnywhereWithSecureRules(GridContainerComponent targetContainer, ItemInstance item)
	{
		if (!IsSecureContainerTarget(targetContainer))
		{
			return targetContainer.TryAddItemAnywhere(item);
		}

		RunSession? runSession = RunSession.Instance;
		string secureContainerId = ResolveSecureContainerId(targetContainer);
		if (runSession == null || !runSession.CanStoreInSecureContainer(secureContainerId, item, out _))
		{
			return false;
		}

		if (!targetContainer.TryAddItemAnywhere(item))
		{
			return false;
		}

		runSession.NotifySecureContainerItemStored(secureContainerId, item);
		return true;
	}

	private bool TrySettleSecureContainerBillForTransfer(ItemInstance item)
	{
		return TrySettleSecureContainerBillForTransfer(item, GetSecureContainerGrid());
	}

	private bool TrySettleSecureContainerBillForTransfer(ItemInstance item, GridContainerComponent? secureContainer)
	{
		RunSession? runSession = RunSession.Instance;
		string secureContainerId = ResolveSecureContainerId(secureContainer);
		if (runSession == null || runSession.GetSecureContainerPendingBill(secureContainerId) <= 0)
		{
			return true;
		}

		if (runSession.TrySettleSecureContainerBill(secureContainerId, out int chargedCredits))
		{
			EmitSignal(
				SignalName.StatusTextChanged,
				UiTextKeys.Inventory.StatusQuickTransferSuccess,
				UiTextArgs.Create(("name", $"{item.Definition.DisplayName} (-{chargedCredits})")));
			return true;
		}

		_customFailureStatusEmitted = true;
		EmitSignal(
				SignalName.StatusTextChanged,
				UiTextKeys.Inventory.StatusSecureStoreFailed,
				UiTextArgs.Create(
					("name", item.Definition.DisplayName),
					("reason", UiText.CurrentLocale == UiText.DefaultLocale ? $"提取前需支付 {runSession.GetSecureContainerPendingBill(secureContainerId)}" : $"Pay {runSession.GetSecureContainerPendingBill(secureContainerId)} Before Retrieval")));
		return false;
	}

	private static string ResolveSecureContainerId(GridContainerComponent? secureContainer)
	{
		return secureContainer?.Definition?.ContainerId ?? RunSession.DefaultSecureContainerId;
	}

	private void EmitSecureStoreFailure(ItemInstance item, SecureContainerStoreFailureReason reason)
	{
		_customFailureStatusEmitted = true;
		EmitSignal(
			SignalName.StatusTextChanged,
			UiTextKeys.Inventory.StatusSecureStoreFailed,
			UiTextArgs.Create(
				("name", item.Definition.DisplayName),
				("reason", GetSecureStoreFailureReasonText(reason))));
	}

	private static string GetSecureStoreFailureReasonText(SecureContainerStoreFailureReason reason)
	{
		bool chinese = UiText.CurrentLocale == UiText.DefaultLocale;
		return reason switch
		{
			SecureContainerStoreFailureReason.NoInsurance => chinese ? "未购买保险" : "No Insurance",
			SecureContainerStoreFailureReason.RentalExpired => chinese ? "时效保险已失效" : "Rental Expired",
			SecureContainerStoreFailureReason.InsufficientQuota => chinese ? "额度不足" : "Insufficient Quota",
			_ => chinese ? "无法存入" : "Cannot Store",
		};
	}

	private void ToggleWarehouseSaleSelection(ContainerItemRecord? record)
	{
		if (record == null)
		{
			return;
		}

		if (!_selectedWarehouseSaleItemIds.Add(record.Item.InstanceId))
		{
			_selectedWarehouseSaleItemIds.Remove(record.Item.InstanceId);
		}
	}

	private void PruneWarehouseSaleSelections()
	{
		GridContainerComponent? container = GetActiveContainerGrid();
		if (container == null || _selectedWarehouseSaleItemIds.Count == 0)
		{
			return;
		}

		System.Collections.Generic.List<string> staleIds = [];
		foreach (string instanceId in _selectedWarehouseSaleItemIds)
		{
			if (!container.ContainsItem(instanceId))
			{
				staleIds.Add(instanceId);
			}
		}

		foreach (string instanceId in staleIds)
		{
			_selectedWarehouseSaleItemIds.Remove(instanceId);
		}
	}

	private void TryUsePlayerInventoryItem(GridContainerComponent sourceContainer, ContainerItemRecord record)
	{
		string itemName = record.Item.Definition.DisplayName;
		if (!ItemUseService.TryUseFromContainer(
			sourceContainer,
			record.Item.InstanceId,
			_uiContext?.Equipment,
			_uiContext?.PlayerHealth,
			_uiContext?.SkillResources,
			_gameplayRoot ?? UiNodeLocator.ResolveGameplayRoot(this),
			_uiContext?.SkillLoadout,
			_uiContext?.SkillCooldowns,
			_uiContext?.AimController,
			_uiContext?.SkillChainTracker,
			out ItemInstance? usedItem,
			out ItemUseFailureReason failureReason))
		{
			EmitSignal(
				SignalName.StatusTextChanged,
				UiTextKeys.Inventory.StatusUseFailure,
				UiTextArgs.Create(
					("name", itemName),
					("reason", failureReason.ToString())));
			return;
		}

		if (!sourceContainer.ContainsItem(record.Item.InstanceId))
		{
			CloseDetails();
		}

		EmitSignal(
			SignalName.StatusTextChanged,
			UiTextKeys.Inventory.StatusUseSuccess,
			UiTextArgs.Create(("name", usedItem?.Definition.DisplayName ?? itemName)));
		NotifyInteractionStateChanged();
	}

	private GridContainerComponent? GetActiveContainerGrid()
	{
		return _uiController?.ActiveExternalContainer;
	}

	private GridContainerComponent? GetSecureContainerGrid()
	{
		return _uiContext?.SecureContainer;
	}

	private void EmitEquipFailure(ItemInstance? item, EquipmentSlotType slotType, EquipmentActionFailureReason reason)
	{
		EmitSignal(
			SignalName.StatusTextChanged,
			UiTextKeys.Inventory.StatusEquipFailure,
			UiTextArgs.Create(
				("name", item?.Definition.DisplayName ?? "-"),
				("slot", EquipmentTextFormatter.GetSlotDisplayName(slotType)),
				("reason", EquipmentTextFormatter.GetFailureReasonText(reason, slotType))));
	}

	private void EmitUnequipFailure(ItemInstance? item, EquipmentSlotType slotType, EquipmentActionFailureReason reason)
	{
		EmitSignal(
			SignalName.StatusTextChanged,
			UiTextKeys.Inventory.StatusUnequipFailure,
			UiTextArgs.Create(
				("name", item?.Definition.DisplayName ?? "-"),
				("slot", EquipmentTextFormatter.GetSlotDisplayName(slotType)),
				("reason", EquipmentTextFormatter.GetFailureReasonText(reason, slotType))));
	}

	private void NotifyInteractionStateChanged()
	{
		EmitSignal(SignalName.InteractionStateChanged);
	}
}
