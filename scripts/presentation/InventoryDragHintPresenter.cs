using Godot;

namespace BattleHarvesterStudy.Presentation;

public partial class InventoryDragHintPresenter : Node
{
	[Export]
	public NodePath DragHintLabelPath { get; set; } = new("../../InventoryUi/DragHint");

	[Export]
	public NodePath UiControllerPath { get; set; } = new("../InventoryUiController");

	[Export]
	public NodePath InteractionControllerPath { get; set; } = new("../InventoryInteractionController");

	private Label? _dragHintLabel;
	private InventoryUiController? _uiController;
	private InventoryInteractionController? _interactionController;

	public override void _Ready()
	{
		_dragHintLabel = GetNodeOrNull<Label>(DragHintLabelPath);
		_uiController = GetNodeOrNull<InventoryUiController>(UiControllerPath);
		_interactionController = GetNodeOrNull<InventoryInteractionController>(InteractionControllerPath);
		SetProcess(true);
	}

	public override void _Process(double delta)
	{
		if (_dragHintLabel == null || _interactionController == null || _uiController == null)
		{
			return;
		}

		if (!_uiController.PlayerWindowOpen || !_interactionController.HasActiveDrag
			|| !_interactionController.TryGetDragHint(out string text, out Vector2 position))
		{
			_dragHintLabel.Visible = false;
			return;
		}

		_dragHintLabel.Visible = true;
		_dragHintLabel.Text = text;
		_dragHintLabel.Position = position;
	}
}
