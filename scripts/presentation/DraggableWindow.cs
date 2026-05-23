using Godot;

namespace BattleHarvesterStudy.Presentation;

public partial class DraggableWindow : PanelContainer
{
	[Export]
	public NodePath DragHandlePath { get; set; } = "Margin/VBox/HeaderBar";

	[Export]
	public bool AllowBodyDrag { get; set; } = true;

	private Control? _dragHandle;
	private bool _isDragging;
	private Vector2 _dragOffset = Vector2.Zero;

	public override void _Ready()
	{
		_dragHandle = GetNodeOrNull<Control>(DragHandlePath);
		if (_dragHandle != null)
		{
			_dragHandle.MouseFilter = MouseFilterEnum.Stop;
			_dragHandle.GuiInput += OnDragHandleGuiInput;
		}

		SetProcess(true);
		SetProcessInput(true);
	}

	public override void _ExitTree()
	{
		if (_dragHandle != null)
		{
			_dragHandle.GuiInput -= OnDragHandleGuiInput;
		}
	}

	public override void _Process(double delta)
	{
		if (!_isDragging)
		{
			return;
		}

		Vector2 mousePosition = GetViewport().GetMousePosition();
		Position = mousePosition - _dragOffset;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton
			&& mouseButton.ButtonIndex == MouseButton.Left)
		{
			if (mouseButton.Pressed)
			{
				if (CanStartBodyDrag(mouseButton.GlobalPosition))
				{
					_isDragging = true;
					_dragOffset = mouseButton.GlobalPosition - GlobalPosition;
				}

				return;
			}

			_isDragging = false;
		}
	}

	private void OnDragHandleGuiInput(InputEvent @event)
	{
		if (@event is not InputEventMouseButton mouseButton || mouseButton.ButtonIndex != MouseButton.Left)
		{
			return;
		}

		if (mouseButton.Pressed)
		{
			_isDragging = true;
			_dragOffset = mouseButton.GlobalPosition - GlobalPosition;
			return;
		}

		_isDragging = false;
	}

	private bool CanStartBodyDrag(Vector2 globalMousePosition)
	{
		if (!AllowBodyDrag || !GetGlobalRect().HasPoint(globalMousePosition))
		{
			return false;
		}

		Control? hovered = GetViewport().GuiGetHoveredControl();
		if (hovered == null)
		{
			return true;
		}

		if (hovered == this || hovered == _dragHandle)
		{
			return true;
		}

		if (!IsAncestorOf(hovered))
		{
			return false;
		}

		if (hovered is Button
			|| hovered is InventoryGridView
			|| hovered is ScrollContainer
			|| hovered is ItemList
			|| hovered is Tree
			|| hovered is TabBar
			|| hovered is TabContainer
			|| hovered is Range
			|| hovered is TextEdit
			|| hovered is LineEdit
			|| hovered is OptionButton
			|| hovered is MenuButton
			|| hovered is CheckBox
			|| hovered is CheckButton
			|| hovered is ColorPicker
			|| hovered is ColorPickerButton
			|| hovered is SpinBox)
		{
			return false;
		}

		return true;
	}
}
