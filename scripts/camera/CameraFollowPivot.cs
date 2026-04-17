using Godot;

namespace BattleHarvesterStudy;

public partial class CameraFollowPivot : Node3D
{
	[Export]
	public Vector3 FollowOffset { get; set; } = new(0.0f, 3.5f, 3.5f);

	[Export]
	public Vector3 FixedRotationDegrees { get; set; } = new(-45.0f, 45.0f, 0.0f);

	private Node3D? _target;

	public override void _Ready()
	{
		TopLevel = true;
		_target = GetParent<Node3D>();
		RotationDegrees = FixedRotationDegrees;
		UpdateTransform();
	}

	public override void _Process(double delta)
	{
		UpdateTransform();
	}

	private void UpdateTransform()
	{
		if (_target == null)
		{
			return;
		}

		GlobalPosition = _target.GlobalPosition + FollowOffset;
		RotationDegrees = FixedRotationDegrees;
	}
}
