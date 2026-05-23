using Godot;
using Godot.Collections;

namespace BattleHarvesterStudy.Presentation;

public partial class InventoryStatusPresenter : Node
{
	[Export]
	public NodePath StatusLabelPath { get; set; } = new("../../InventoryUi/StatusLabel");

	[Export]
	public NodePath UiControllerPath { get; set; } = new("../InventoryUiController");

	[Export]
	public NodePath InteractionControllerPath { get; set; } = new("../InventoryInteractionController");

	private Label? _statusLabel;
	private InventoryUiController? _uiController;
	private InventoryInteractionController? _interactionController;
	private string _lastTextKey = UiTextKeys.Inventory.StatusClosedInventory;
	private Dictionary<string, Variant> _lastFormatArgs = [];

	public override void _Ready()
	{
		_statusLabel = GetNodeOrNull<Label>(StatusLabelPath);
		_uiController = GetNodeOrNull<InventoryUiController>(UiControllerPath);
		_interactionController = GetNodeOrNull<InventoryInteractionController>(InteractionControllerPath);

		if (_uiController != null)
		{
			_uiController.StatusTextChanged += OnStatusTextChanged;
		}

		if (_interactionController != null)
		{
			_interactionController.StatusTextChanged += OnStatusTextChanged;
		}

		UiText.LanguageChanged += OnLanguageChanged;
		ApplyCurrentText();
	}

	public override void _ExitTree()
	{
		if (_uiController != null)
		{
			_uiController.StatusTextChanged -= OnStatusTextChanged;
		}

		if (_interactionController != null)
		{
			_interactionController.StatusTextChanged -= OnStatusTextChanged;
		}

		UiText.LanguageChanged -= OnLanguageChanged;
	}

	private void OnStatusTextChanged(string textKey, Dictionary<string, Variant> formatArgs)
	{
		_lastTextKey = textKey;
		_lastFormatArgs = formatArgs;
		ApplyCurrentText();
	}

	private void OnLanguageChanged()
	{
		ApplyCurrentText();
	}

	private void ApplyCurrentText()
	{
		if (_statusLabel == null)
		{
			return;
		}

		_statusLabel.Text = UiText.Resolve(_lastTextKey, _lastFormatArgs);
	}
}
