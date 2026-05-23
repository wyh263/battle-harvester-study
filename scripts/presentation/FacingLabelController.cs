using Godot;

namespace BattleHarvesterStudy.Presentation;

public partial class FacingLabelController : Node
{
	private Label3D? _label;

	public override void _Ready()
	{
		Node3D? owner = GetOwner<Node3D>();
		_label = owner?.GetNodeOrNull<Label3D>("Visuals/DirectionLabel");
	}

	public void SetFacingLabel(string label)
	{
		if (_label == null)
		{
			return;
		}

		_label.Text = label;
	}
}
