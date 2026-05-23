using Godot;

namespace BattleHarvesterStudy.Camera;

public partial class CameraFollowPivot : Node3D
{
	[Export]
	public float CameraDistance { get; set; } = 25.0f;

	[Export]
	public Vector3 FocusOffset { get; set; } = new(0.0f, 0.9f, 0.0f);

	[Export]
	public float FocusLeadDistance { get; set; } = 0.5f;

	[Export]
	public Vector3 FixedRotationDegrees { get; set; } = new(-38.0f, 0.0f, 0.0f);

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

		RotationDegrees = FixedRotationDegrees;
		Basis basis = Transform.Basis;
		Vector3 groundForward = -basis.Z;
		groundForward.Y = 0.0f;
		groundForward = groundForward.LengthSquared() > 0.0f
			? groundForward.Normalized()
			: Vector3.Forward;
		Vector3 focusPoint = _target.GlobalPosition + FocusOffset + (groundForward * FocusLeadDistance);
		GlobalPosition = focusPoint + (basis.Z * CameraDistance);
	}
}
