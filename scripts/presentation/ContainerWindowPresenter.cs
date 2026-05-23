using Godot;

namespace BattleHarvesterStudy.Presentation;

public partial class ContainerWindowPresenter : Node
{
	[Signal]
	public delegate void SellModePressedEventHandler();

	[Signal]
	public delegate void SellSelectedPressedEventHandler();

	[Export]
	public NodePath HeaderLabelPath { get; set; } = new("../../InventoryUi/ContainerWindow/Margin/VBox/HeaderBar/Header");

	[Export]
	public NodePath SellModeButtonPath { get; set; } = new("../../InventoryUi/ContainerWindow/Margin/VBox/HeaderBar/SellModeButton");

	[Export]
	public NodePath SellSelectedButtonPath { get; set; } = new("../../InventoryUi/ContainerWindow/Margin/VBox/HeaderBar/SellSelectedButton");

	[Export]
	public NodePath SummaryLabelPath { get; set; } = new("../../InventoryUi/ContainerWindow/Margin/VBox/Summary");

	[Export]
	public NodePath GridViewPath { get; set; } = new("../../InventoryUi/ContainerWindow/Margin/VBox/Grid");

	private Label? _headerLabel;
	private Label? _summaryLabel;
	private InventoryGridView? _gridView;
	private Button? _sellModeButton;
	private Button? _sellSelectedButton;

	public override void _Ready()
	{
		_headerLabel = GetNodeOrNull<Label>(HeaderLabelPath);
		_sellModeButton = GetNodeOrNull<Button>(SellModeButtonPath);
		_sellSelectedButton = GetNodeOrNull<Button>(SellSelectedButtonPath);
		_summaryLabel = GetNodeOrNull<Label>(SummaryLabelPath);
		_gridView = GetNodeOrNull<InventoryGridView>(GridViewPath);

		if (_sellModeButton != null)
		{
			_sellModeButton.Pressed += () => EmitSignal(SignalName.SellModePressed);
		}

		if (_sellSelectedButton != null)
		{
			_sellSelectedButton.Pressed += () => EmitSignal(SignalName.SellSelectedPressed);
		}
	}

	public void Present(
		string? activeContainerName,
		GridContainerComponent? container,
		Vector2I selectedCell,
		ContainerItemRecord? selectedRecord,
		bool isFocused,
		Rect2I? previewRect,
		bool previewValid,
		bool isWarehouse,
		bool sellModeActive,
		int selectedSellCount,
		int selectedSellValue)
	{
		if (_headerLabel != null)
		{
			_headerLabel.Text = string.IsNullOrWhiteSpace(activeContainerName)
				? UiText.Resolve(UiTextKeys.Inventory.HeaderContainer)
				: activeContainerName;
		}

		if (_summaryLabel != null)
		{
			_summaryLabel.Text = BuildSummary(container, selectedRecord, isWarehouse, sellModeActive, selectedSellCount, selectedSellValue);
		}

		if (_sellModeButton != null)
		{
			_sellModeButton.Visible = isWarehouse;
			_sellModeButton.Text = sellModeActive
				? UiText.Resolve(UiTextKeys.Inventory.StopSellingButton)
				: UiText.Resolve(UiTextKeys.Inventory.SellButton);
		}

		if (_sellSelectedButton != null)
		{
			_sellSelectedButton.Visible = isWarehouse && sellModeActive && selectedSellCount > 0;
			_sellSelectedButton.Text = UiText.Resolve(
				UiTextKeys.Inventory.SellSelectedButton,
				("count", selectedSellCount),
				("value", selectedSellValue));
		}

		_gridView?.SetContainer(container);
		_gridView?.SetSelection(selectedCell, selectedRecord, isFocused);
		_gridView?.SetPreview(previewRect, previewValid);
	}

	private static string BuildSummary(
		GridContainerComponent? container,
		ContainerItemRecord? selectedRecord,
		bool isWarehouse,
		bool sellModeActive,
		int selectedSellCount,
		int selectedSellValue)
	{
		if (container == null || !container.IsConfigured)
		{
			return UiText.Resolve(UiTextKeys.Inventory.SummaryClosed);
		}

		string selectedText = selectedRecord == null
			? UiText.Resolve(UiTextKeys.Inventory.SelectedEmpty)
			: container.SearchRuntime?.CanInteractWithItem(selectedRecord.Item.InstanceId) == false
				? UiText.Resolve(UiTextKeys.Inventory.SelectedUnsearched)
				: UiText.Resolve(
					UiTextKeys.Inventory.SelectedItem,
					("name", ContentTextFormatter.GetItemDisplayName(selectedRecord.Item.Definition)),
					("count", selectedRecord.Item.StackCount));

		string summary = UiText.Resolve(
			UiTextKeys.Inventory.ContainerSummary,
			("title", container.Definition?.DisplayName ?? UiText.Resolve(UiTextKeys.Inventory.HeaderContainer)),
			("item_count", container.ItemRecords.Count),
			("selected", selectedText),
			("drag_hint", UiText.Resolve(UiTextKeys.Inventory.HintDragItem)),
			("rotate_hint", UiText.Resolve(UiTextKeys.Inventory.HintRotateDragging)),
			("quick_transfer_hint", UiText.Resolve(UiTextKeys.Inventory.HintQuickTransfer)),
			("close_hint", UiText.Resolve(UiTextKeys.Inventory.HintCloseContainer)));

		if (!isWarehouse)
		{
			return summary;
		}

		string sellLine = sellModeActive
			? UiText.Resolve(
				UiTextKeys.Inventory.WarehouseModeActive,
				("count", selectedSellCount),
				("value", selectedSellValue))
			: UiText.Resolve(UiTextKeys.Inventory.WarehouseModeInactive);

		return $"{summary}\n{sellLine}";
	}
}
