using Godot;

namespace BattleHarvesterStudy.Presentation;

public partial class PlayerInventoryWindowPresenter : Node
{
	[Export]
	public NodePath UiContextPath { get; set; } = new("../PlayerUiContext");

	[Export]
	public NodePath HeaderLabelPath { get; set; } = new("../../InventoryUi/PlayerInventoryWindow/Margin/VBox/HeaderBar/Header");

	[Export]
	public NodePath RunLootLabelPath { get; set; } = new("../../InventoryUi/PlayerInventoryWindow/Margin/VBox/HeaderBar/RunLootLabel");

	[Export]
	public NodePath SummaryLabelPath { get; set; } = new("../../InventoryUi/PlayerInventoryWindow/Margin/VBox/ContentRow/InventoryColumn/Summary");

	[Export]
	public NodePath GridViewPath { get; set; } = new("../../InventoryUi/PlayerInventoryWindow/Margin/VBox/ContentRow/InventoryColumn/Grid");

	private Label? _headerLabel;
	private Label? _runLootLabel;
	private Label? _summaryLabel;
	private InventoryGridView? _gridView;
	private PlayerUiContext? _uiContext;

	public override void _Ready()
	{
		_uiContext = GetNodeOrNull<PlayerUiContext>(UiContextPath);
		_headerLabel = GetNodeOrNull<Label>(HeaderLabelPath);
		_runLootLabel = GetNodeOrNull<Label>(RunLootLabelPath);
		_summaryLabel = GetNodeOrNull<Label>(SummaryLabelPath);
		_gridView = GetNodeOrNull<InventoryGridView>(GridViewPath);
	}

	public void Present(
		GridContainerComponent? container,
		Vector2I selectedCell,
		ContainerItemRecord? selectedRecord,
		bool isFocused,
		Rect2I? previewRect,
		bool previewValid)
	{
		if (_headerLabel != null)
		{
			_headerLabel.Text = UiText.Resolve(UiTextKeys.Inventory.HeaderPlayer);
		}

		if (_runLootLabel != null)
		{
			int runLootValue = GetCurrentRunLootValue();
			_runLootLabel.Text = UiText.Resolve(UiTextKeys.Inventory.RunLoot, ("value", runLootValue));
		}

		if (_summaryLabel != null)
		{
			_summaryLabel.Text = BuildSummary(container, selectedRecord);
		}

		_gridView?.SetContainer(container);
		_gridView?.SetSelection(selectedCell, selectedRecord, isFocused);
		_gridView?.SetPreview(previewRect, previewValid);
	}

	private static string BuildSummary(GridContainerComponent? container, ContainerItemRecord? selectedRecord)
	{
		if (container == null || !container.IsConfigured)
		{
			return UiText.Resolve(UiTextKeys.Inventory.SummaryUnconfigured);
		}

		int usedCells = 0;
		foreach (ContainerItemRecord record in container.ItemRecords)
		{
			Vector2I footprint = record.Item.GetFootprint();
			usedCells += footprint.X * footprint.Y;
		}

		Vector2I size = container.GetGridSize();
		string selectedText = selectedRecord == null
			? UiText.Resolve(UiTextKeys.Inventory.SelectedEmpty)
			: UiText.Resolve(
				UiTextKeys.Inventory.SelectedItem,
				("name", ContentTextFormatter.GetItemDisplayName(selectedRecord.Item.Definition)),
				("count", selectedRecord.Item.StackCount));

		return UiText.Resolve(
			UiTextKeys.Inventory.PlayerSummary,
			("title", UiText.Resolve(UiTextKeys.Inventory.HeaderPlayer)),
			("item_count", container.ItemRecords.Count),
			("used_cells", usedCells),
			("total_cells", size.X * size.Y),
			("selected", selectedText),
			("toggle_hint", UiText.Resolve(UiTextKeys.Inventory.HintToggleInventory)),
			("take_all_hint", UiText.Resolve(UiTextKeys.Inventory.HintTakeAll)),
			("quick_transfer_hint", UiText.Resolve(UiTextKeys.Inventory.HintQuickTransfer)));
	}

	private int GetCurrentRunLootValue()
	{
		int total = 0;
		if (_uiContext?.Inventory?.GetPrimaryContainer() is GridContainerComponent inventoryContainer)
		{
			total += SumRunLootValue(inventoryContainer);
		}

		if (_uiContext?.SecureContainer is GridContainerComponent secureContainer)
		{
			total += SumRunLootValue(secureContainer);
		}

		if (_uiContext?.Equipment != null)
		{
			foreach ((EquipmentSlotType _, EquipmentSlotRecord slot) in _uiContext.Equipment.Slots)
			{
				if (slot.EquippedItem?.CountsAsRunLoot == true)
				{
					total += ItemValueClassifier.GetRunLootMarketValue(slot.EquippedItem);
				}
			}
		}

		return total;
	}

	private static int SumRunLootValue(GridContainerComponent container)
	{
		int total = 0;
		foreach (ContainerItemRecord record in container.ItemRecords)
		{
			if (!record.Item.CountsAsRunLoot)
			{
				continue;
			}

			total += ItemValueClassifier.GetRunLootMarketValue(record.Item);
		}

		return total;
	}
}
