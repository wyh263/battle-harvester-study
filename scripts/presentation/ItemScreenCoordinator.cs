using Godot;

namespace BattleHarvesterStudy.Presentation;

public partial class ItemScreenCoordinator : Node
{
	[Export]
	public NodePath UiContextPath { get; set; } = new("../PlayerUiContext");

	[Export]
	public NodePath UiRootPath { get; set; } = new("../../InventoryUi");

	[Export]
	public NodePath PlayerWindowPath { get; set; } = new("PlayerInventoryWindow");

	[Export]
	public NodePath ContainerWindowPath { get; set; } = new("ContainerWindow");

	[Export]
	public NodePath DetailsWindowPath { get; set; } = new("ItemDetailsWindow");

	[Export]
	public NodePath EquipmentWindowPath { get; set; } = new("PlayerInventoryWindow/Margin/VBox/ContentRow/EquipmentColumn/EquipmentPanel");

	[Export]
	public NodePath PlayerCloseButtonPath { get; set; } = new("PlayerInventoryWindow/Margin/VBox/HeaderBar/CloseButton");

	[Export]
	public NodePath ContainerCloseButtonPath { get; set; } = new("ContainerWindow/Margin/VBox/HeaderBar/CloseButton");

	[Export]
	public NodePath DetailsCloseButtonPath { get; set; } = new("ItemDetailsWindow/Margin/VBox/HeaderBar/CloseButton");

	[Export]
	public NodePath PlayerGridViewPath { get; set; } = new("PlayerInventoryWindow/Margin/VBox/ContentRow/InventoryColumn/Grid");

	[Export]
	public NodePath ContainerGridViewPath { get; set; } = new("ContainerWindow/Margin/VBox/Grid");

	[Export]
	public NodePath SecureContainerPanelPath { get; set; } = new("PlayerInventoryWindow/Margin/VBox/ContentRow/InventoryColumn/SecureContainerPanel");

	[Export]
	public NodePath SecureGridViewPath { get; set; } = new("PlayerInventoryWindow/Margin/VBox/ContentRow/InventoryColumn/SecureContainerPanel/Margin/SecureVBox/SecureGrid");

	[Export]
	public NodePath UiControllerPath { get; set; } = new("../InventoryUiController");

	[Export]
	public NodePath InteractionControllerPath { get; set; } = new("../InventoryInteractionController");

	[Export]
	public NodePath PlayerWindowPresenterPath { get; set; } = new("../PlayerInventoryWindowPresenter");

	[Export]
	public NodePath ContainerWindowPresenterPath { get; set; } = new("../ContainerWindowPresenter");

	[Export]
	public NodePath EquipmentPanelPresenterPath { get; set; } = new("../EquipmentPanelPresenter");

	[Export]
	public NodePath PlayerStatusPanelPresenterPath { get; set; } = new("../PlayerStatusPanelPresenter");

	private Control? _uiRoot;
	private Control? _playerWindow;
	private Control? _containerWindow;
	private Control? _detailsWindow;
	private Vector2 _detailsWindowBasePosition;
	private Control? _equipmentWindow;
	private Button? _playerCloseButton;
	private Button? _containerCloseButton;
	private Button? _detailsCloseButton;
	private InventoryGridView? _playerGridView;
	private InventoryGridView? _containerGridView;
	private Control? _secureContainerPanel;
	private InventoryGridView? _secureGridView;
	private InventoryComponent? _playerInventory;
	private PlayerUiContext? _uiContext;
	private InventoryUiController? _uiController;
	private InventoryInteractionController? _interactionController;
	private PlayerInventoryWindowPresenter? _playerWindowPresenter;
	private ContainerWindowPresenter? _containerWindowPresenter;
	private EquipmentPanelPresenter? _equipmentPanelPresenter;
	private PlayerStatusPanelPresenter? _playerStatusPanelPresenter;
	private bool _needsRefresh = true;
	private readonly System.Collections.Generic.List<Control> _detailWindows = [];
	private readonly System.Collections.Generic.List<Label?> _detailSummaryLabels = [];

	public override void _Ready()
	{
		Node3D? gameplayRoot = UiNodeLocator.ResolveGameplayRoot(this);
		_uiContext = GetNodeOrNull<PlayerUiContext>(UiContextPath);
		_uiRoot = GetNodeOrNull<Control>(UiRootPath);
		_playerWindow = GetUiNodeOrNull<Control>(PlayerWindowPath);
		_containerWindow = GetUiNodeOrNull<Control>(ContainerWindowPath);
		_detailsWindow = GetUiNodeOrNull<Control>(DetailsWindowPath);
		_detailsWindowBasePosition = _detailsWindow?.Position ?? Vector2.Zero;
		_equipmentWindow = GetUiNodeOrNull<Control>(EquipmentWindowPath);
		_playerCloseButton = GetUiNodeOrNull<Button>(PlayerCloseButtonPath);
		_containerCloseButton = GetUiNodeOrNull<Button>(ContainerCloseButtonPath);
		_detailsCloseButton = GetUiNodeOrNull<Button>(DetailsCloseButtonPath);
		_playerGridView = GetUiNodeOrNull<InventoryGridView>(PlayerGridViewPath);
		_containerGridView = GetUiNodeOrNull<InventoryGridView>(ContainerGridViewPath);
		_secureContainerPanel = GetUiNodeOrNull<Control>(SecureContainerPanelPath);
		_secureGridView = GetUiNodeOrNull<InventoryGridView>(SecureGridViewPath);
		_playerInventory = _uiContext?.Inventory;
		_uiController = GetNodeOrNull<InventoryUiController>(UiControllerPath);
		_interactionController = GetNodeOrNull<InventoryInteractionController>(InteractionControllerPath);
		_playerWindowPresenter = GetNodeOrNull<PlayerInventoryWindowPresenter>(PlayerWindowPresenterPath);
		_containerWindowPresenter = GetNodeOrNull<ContainerWindowPresenter>(ContainerWindowPresenterPath);
		_equipmentPanelPresenter = GetNodeOrNull<EquipmentPanelPresenter>(EquipmentPanelPresenterPath);
		_playerStatusPanelPresenter = GetNodeOrNull<PlayerStatusPanelPresenter>(PlayerStatusPanelPresenterPath);

		_interactionController?.BindDependencies(
			gameplayRoot,
			_uiContext,
			_uiController,
			_playerGridView,
			_containerGridView,
			_secureGridView,
			_equipmentPanelPresenter,
			this);

		if (_uiController != null)
		{
			_uiController.UiStateChanged += OnUiStateChanged;
		}
		UiText.LanguageChanged += OnLanguageChanged;
		if (_interactionController != null)
		{
			_interactionController.InteractionStateChanged += OnInteractionStateChanged;
		}
		if (_containerWindowPresenter != null)
		{
			_containerWindowPresenter.SellModePressed += OnContainerSellModePressed;
			_containerWindowPresenter.SellSelectedPressed += OnContainerSellSelectedPressed;
		}
		if (_playerCloseButton != null)
		{
			_playerCloseButton.Pressed += OnPlayerClosePressed;
		}
		if (_containerCloseButton != null)
		{
			_containerCloseButton.Pressed += OnContainerClosePressed;
		}
		if (_detailsCloseButton != null)
		{
			_detailsCloseButton.Pressed += OnDetailsClosePressed;
		}

		ConnectGridSignals(_playerGridView, InventoryInteractionController.GridSide.Player);
		ConnectGridSignals(_containerGridView, InventoryInteractionController.GridSide.Container);
		ConnectGridSignals(_secureGridView, InventoryInteractionController.GridSide.Secure);
		SetProcess(true);
		ApplyVisibilityState(BuildVisibilityState());
	}

	public override void _ExitTree()
	{
		if (_uiController != null)
		{
			_uiController.UiStateChanged -= OnUiStateChanged;
		}
		UiText.LanguageChanged -= OnLanguageChanged;
		if (_interactionController != null)
		{
			_interactionController.InteractionStateChanged -= OnInteractionStateChanged;
		}
		if (_containerWindowPresenter != null)
		{
			_containerWindowPresenter.SellModePressed -= OnContainerSellModePressed;
			_containerWindowPresenter.SellSelectedPressed -= OnContainerSellSelectedPressed;
		}
		if (_playerCloseButton != null)
		{
			_playerCloseButton.Pressed -= OnPlayerClosePressed;
		}
		if (_containerCloseButton != null)
		{
			_containerCloseButton.Pressed -= OnContainerClosePressed;
		}
		if (_detailsCloseButton != null)
		{
			_detailsCloseButton.Pressed -= OnDetailsClosePressed;
		}
	}

	public override void _Process(double delta)
	{
		ItemScreenVisibilityState visibilityState = BuildVisibilityState();
		if (!visibilityState.RootVisible)
		{
			ApplyVisibilityState(visibilityState);
			return;
		}

		if (_needsRefresh || (_interactionController?.HasActiveDrag ?? false))
		{
			RefreshPanels();
			_needsRefresh = false;
		}

		ApplyVisibilityState(visibilityState);
	}

	protected virtual ItemScreenVisibilityState BuildVisibilityState()
	{
		bool playerWindowOpen = IsPlayerWindowOpen();
		bool hasActiveContainer = GetActiveContainerGrid() != null;
		return new ItemScreenVisibilityState(
			RootVisible: playerWindowOpen || hasActiveContainer,
			PlayerPanelVisible: playerWindowOpen,
			ExternalContainerPanelVisible: playerWindowOpen && hasActiveContainer,
			SecureContainerPanelVisible: playerWindowOpen,
			DetailsPanelVisible: false,
			EquipmentPanelVisible: playerWindowOpen);
	}

	protected virtual void RefreshPanels()
	{
		GridContainerComponent? playerGrid = _playerInventory?.GetPrimaryContainer();
		GridContainerComponent? containerGrid = GetActiveContainerGrid();
		Vector2I playerSelectedCell = ClampSelectedCell(
			_interactionController?.GetSelectedCell(InventoryInteractionController.GridSide.Player) ?? Vector2I.Zero,
			playerGrid);
		Vector2I containerSelectedCell = ClampSelectedCell(
			_interactionController?.GetSelectedCell(InventoryInteractionController.GridSide.Container) ?? Vector2I.Zero,
			containerGrid);
		GridContainerComponent? secureGrid = _uiContext?.SecureContainer;
		Vector2I secureSelectedCell = ClampSelectedCell(
			_interactionController?.GetSelectedCell(InventoryInteractionController.GridSide.Secure) ?? Vector2I.Zero,
			secureGrid);
		ContainerItemRecord? playerSelectedRecord = playerGrid?.FindItemAt(playerSelectedCell);
		ContainerItemRecord? containerSelectedRecord = containerGrid?.FindItemAt(containerSelectedCell);
		ContainerItemRecord? secureSelectedRecord = secureGrid?.FindItemAt(secureSelectedCell);
		Rect2I? playerPreview = null;
		bool playerPreviewValid = true;
		Rect2I? containerPreview = null;
		bool containerPreviewValid = true;
		Rect2I? securePreview = null;
		bool securePreviewValid = true;
		if (_interactionController != null)
		{
			_interactionController.BuildPreviews(
				out containerPreview,
				out containerPreviewValid,
				out securePreview,
				out securePreviewValid,
				out playerPreview,
				out playerPreviewValid);
		}

		_playerWindowPresenter?.Present(
			playerGrid,
			playerSelectedCell,
			playerSelectedRecord,
			_interactionController?.FocusedSide == InventoryInteractionController.GridSide.Player,
			playerPreview,
			playerPreviewValid);
		_containerWindowPresenter?.Present(
			_uiController?.ActiveExternalContainerName,
			containerGrid,
			containerSelectedCell,
			containerSelectedRecord,
			_interactionController?.FocusedSide == InventoryInteractionController.GridSide.Container,
			containerPreview,
			containerPreviewValid,
			_interactionController?.IsWarehouseContainerActive() == true,
			_interactionController?.WarehouseSellModeActive == true,
			_interactionController?.GetSelectedWarehouseSaleCount() ?? 0,
			_interactionController?.GetSelectedWarehouseSaleValue() ?? 0);
		_secureGridView?.SetContainer(secureGrid);
		_secureGridView?.SetSelection(
			secureSelectedCell,
			secureSelectedRecord,
			_interactionController?.FocusedSide == InventoryInteractionController.GridSide.Secure);
		_secureGridView?.SetPreview(securePreview, securePreviewValid);
		_containerGridView?.SetMarkedItems(_interactionController?.WarehouseSellModeActive == true
			? _interactionController.SelectedWarehouseSaleItemIds
			: null);
		_playerGridView?.SetMarkedItems(null);
		_secureGridView?.SetMarkedItems(null);
		RefreshDetailWindows();
		_equipmentPanelPresenter?.Present();
		_playerStatusPanelPresenter?.Present();
	}

	private void RefreshDetailWindows()
	{
		if (_interactionController == null || _detailsWindow == null)
		{
			return;
		}

		System.Collections.Generic.IReadOnlyList<InventoryInteractionController.DetailTarget> targets = _interactionController.DetailTargets;
		EnsureDetailWindowCount(targets.Count);
		for (int index = 0; index < _detailWindows.Count; index++)
		{
			Control window = _detailWindows[index];
			if (index >= targets.Count)
			{
				window.Visible = false;
				continue;
			}

			InventoryInteractionController.DetailTarget target = targets[index];
			if (!_interactionController.TryResolveDetailRecord(target, out ContainerItemRecord? record) || record == null)
			{
				window.Visible = false;
				continue;
			}

			bool wasVisible = window.Visible;
			window.Visible = true;
			if (!wasVisible)
			{
				window.Position = _detailsWindowBasePosition + new Vector2(24.0f * index, 24.0f * index);
			}
			if (_detailSummaryLabels[index] != null)
			{
				_detailSummaryLabels[index]!.Text = ItemDetailsPanelPresenter.BuildSummaryText(record);
			}
			ItemDetailsPanelPresenter.RefreshWeaponSkillButtons(window, record.Item);
		}
	}

	private void EnsureDetailWindowCount(int count)
	{
		if (_detailsWindow == null || _uiRoot == null)
		{
			return;
		}

		while (_detailWindows.Count < count)
		{
			Control duplicate = (Control)_detailsWindow.Duplicate();
			duplicate.Name = $"ItemDetailsWindow{_detailWindows.Count + 1}";
			duplicate.Visible = false;
			_uiRoot.AddChild(duplicate);

			int detailIndex = _detailWindows.Count;
			Button? closeButton = duplicate.GetNodeOrNull<Button>("Margin/VBox/HeaderBar/CloseButton");
			if (closeButton != null)
			{
				closeButton.Pressed += () => OnDynamicDetailClose(detailIndex);
			}

			VBoxContainer? slotsContainer = duplicate.GetNodeOrNull<VBoxContainer>("Margin/VBox/WeaponSkillSlots");
			if (slotsContainer != null)
			{
				for (int slotIndex = 0; slotIndex < slotsContainer.GetChildCount(); slotIndex++)
				{
					if (slotsContainer.GetChild(slotIndex) is not Button slotButton)
					{
						continue;
					}

					int capturedSlotIndex = slotIndex;
					slotButton.GuiInput += inputEvent => OnDynamicWeaponSkillSlotInput(detailIndex, capturedSlotIndex, inputEvent);
				}
			}

			_detailWindows.Add(duplicate);
			_detailSummaryLabels.Add(duplicate.GetNodeOrNull<Label>("Margin/VBox/Summary"));
		}
	}

	public bool TryGetWeaponSkillSlotTargetFromGlobalPosition(
		Vector2 globalPosition,
		out InventoryInteractionController.DetailTarget target,
		out int slotIndex)
	{
		target = default;
		slotIndex = -1;
		if (_interactionController == null)
		{
			return false;
		}

		System.Collections.Generic.IReadOnlyList<InventoryInteractionController.DetailTarget> targets = _interactionController.DetailTargets;
		for (int detailIndex = 0; detailIndex < targets.Count && detailIndex < _detailWindows.Count; detailIndex++)
		{
			Control window = _detailWindows[detailIndex];
			if (!window.Visible)
			{
				continue;
			}

			VBoxContainer? slotsContainer = window.GetNodeOrNull<VBoxContainer>("Margin/VBox/WeaponSkillSlots");
			if (slotsContainer == null || !slotsContainer.Visible)
			{
				continue;
			}

			for (int childIndex = 0; childIndex < slotsContainer.GetChildCount(); childIndex++)
			{
				if (slotsContainer.GetChild(childIndex) is not Button button || !button.Visible)
				{
					continue;
				}

				if (!button.GetGlobalRect().HasPoint(globalPosition))
				{
					continue;
				}

				target = targets[detailIndex];
				slotIndex = childIndex;
				return true;
			}
		}

		return false;
	}

	private void ConnectGridSignals(InventoryGridView? gridView, InventoryInteractionController.GridSide side)
	{
		if (gridView == null)
		{
			return;
		}

		gridView.CellPressed += (cell, buttonIndex) => _interactionController?.HandleGridCellPressed(side, cell, buttonIndex);
		gridView.CellReleased += (cell, buttonIndex) => _interactionController?.HandleGridCellReleased(side, cell, buttonIndex);
		gridView.CellDoubleClicked += (cell, buttonIndex) => _interactionController?.HandleGridCellDoubleClicked(side, cell, buttonIndex);
		gridView.CellHovered += cell => _interactionController?.HandleGridCellHovered(side, cell);
		gridView.PointerExited += () => _interactionController?.HandleGridPointerExited(side);
	}

	private void ApplyVisibilityState(ItemScreenVisibilityState visibilityState)
	{
		if (_uiRoot != null)
		{
			_uiRoot.Visible = visibilityState.RootVisible;
		}

		if (_playerWindow != null)
		{
			_playerWindow.Visible = visibilityState.PlayerPanelVisible;
		}

		if (_containerWindow != null)
		{
			_containerWindow.Visible = visibilityState.ExternalContainerPanelVisible;
		}

		if (_secureContainerPanel != null)
		{
			_secureContainerPanel.Visible = visibilityState.SecureContainerPanelVisible;
		}

		if (_detailsWindow != null)
		{
			_detailsWindow.Visible = false;
		}

		if (_equipmentWindow != null)
		{
			_equipmentWindow.Visible = visibilityState.EquipmentPanelVisible;
		}
	}

	private void OnUiStateChanged()
	{
		_needsRefresh = true;
		ApplyVisibilityState(BuildVisibilityState());
	}

	private void OnInteractionStateChanged()
	{
		_needsRefresh = true;
	}

	private void OnLanguageChanged()
	{
		_needsRefresh = true;
	}

	private void OnPlayerClosePressed()
	{
		_uiController?.ClosePlayerInventoryWindow();
	}

	private void OnContainerClosePressed()
	{
		_uiController?.CloseContainerWindow();
	}

	private void OnContainerSellModePressed()
	{
		_interactionController?.ToggleWarehouseSellMode();
	}

	private void OnContainerSellSelectedPressed()
	{
		_interactionController?.SellSelectedWarehouseItems();
	}

	private void OnDetailsClosePressed()
	{
		_interactionController?.CloseDetails();
		ApplyVisibilityState(BuildVisibilityState());
	}

	private void OnDynamicDetailClose(int detailIndex)
	{
		if (_interactionController == null || detailIndex < 0 || detailIndex >= _interactionController.DetailTargets.Count)
		{
			return;
		}

		_interactionController.CloseDetailTarget(_interactionController.DetailTargets[detailIndex]);
	}

	private void OnDynamicWeaponSkillSlotInput(int detailIndex, int slotIndex, InputEvent inputEvent)
	{
		if (_interactionController == null
			|| inputEvent is not InputEventMouseButton mouseButton
			|| !mouseButton.Pressed
			|| detailIndex < 0
			|| detailIndex >= _interactionController.DetailTargets.Count)
		{
			return;
		}

		_interactionController.HandleWeaponSkillSlotPressed(
			_interactionController.DetailTargets[detailIndex],
			slotIndex,
			(int)mouseButton.ButtonIndex);
	}

	private static Vector2I ClampSelectedCell(Vector2I selectedCell, GridContainerComponent? container)
	{
		Vector2I size = container?.GetGridSize() ?? Vector2I.Zero;
		if (size == Vector2I.Zero)
		{
			return Vector2I.Zero;
		}

		return new Vector2I(
			Mathf.Clamp(selectedCell.X, 0, size.X - 1),
			Mathf.Clamp(selectedCell.Y, 0, size.Y - 1));
	}

	private bool IsPlayerWindowOpen()
	{
		return _uiController?.PlayerWindowOpen ?? false;
	}

	private GridContainerComponent? GetActiveContainerGrid()
	{
		return _uiController?.ActiveExternalContainer;
	}

	private T? GetUiNodeOrNull<T>(NodePath path)
		where T : class
	{
		T? directNode = GetNodeOrNull<T>(path);
		if (directNode != null)
		{
			return directNode;
		}

		return _uiRoot?.GetNodeOrNull<T>(path);
	}
}
