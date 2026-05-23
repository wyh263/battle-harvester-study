using Godot;

namespace BattleHarvesterStudy.Locomotion;

public partial class MoveState : BattleHarvesterStudy.State.State
{
	public override void Enter()
	{
		base.Enter();
		Actor.DesiredMoveDirection = Vector3.Zero;
		GD.Print("Enter Move");
	}

	public override void Exit()
	{
		base.Exit();
		Actor.RemoveMoveSpeedModifier(MovementModifierKeys.Run);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Actor.IsGameplayInputBlocked)
		{
			Actor.DesiredMoveDirection = Vector3.Zero;
			Actor.RemoveMoveSpeedModifier(MovementModifierKeys.Run);
			Machine.ChangeState("Idle");
			return;
		}

		if (Input.IsActionJustPressed("dash") && Actor.CanStartDash())
		{
			Machine.ChangeState("Dash");
			return;
		}

		if (Actor.SkillLoadout.HasQueuedSkill)
		{
			Machine.ChangeState("Attack");
			return;
		}

		Vector2 input = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		Vector3 direction = CalculateCameraRelativeDirection(input);
		Actor.DesiredMoveDirection = direction;

		if (Input.IsActionPressed("run"))
		{
			Actor.SetMoveSpeedModifier(MovementModifierKeys.Run, multiplier: Actor.GetRunMultiplier());
		}
		else
		{
			Actor.RemoveMoveSpeedModifier(MovementModifierKeys.Run);
		}

		if (direction != Vector3.Zero)
		{
			Actor.RememberMoveDirection(direction);
			RotateTowardMovement(direction);
			Actor.FacingLabel = ResolveFacingLabel(input);
		}
		else
		{
			Actor.DesiredMoveDirection = Vector3.Zero;
			Actor.RemoveMoveSpeedModifier(MovementModifierKeys.Run);
			Machine.ChangeState("Idle");
		}
	}

	private Vector3 CalculateCameraRelativeDirection(Vector2 input)
	{
		Camera3D? camera = GetViewport().GetCamera3D();
		if (camera == null || input == Vector2.Zero)
		{
			return Vector3.Zero;
		}

		Vector3 forward = -camera.GlobalTransform.Basis.Z;
		Vector3 right = camera.GlobalTransform.Basis.X;

		forward.Y = 0.0f;
		right.Y = 0.0f;

		forward = forward.Normalized();
		right = right.Normalized();

		Vector3 direction = right * input.X - forward * input.Y;

		if (direction.LengthSquared() > 1.0f)
		{
			direction = direction.Normalized();
		}

		return direction;
	}

	private void RotateTowardMovement(Vector3 direction)
	{
		float targetAngle = Mathf.Atan2(-direction.X, -direction.Z);

		EntityNode.Rotation = new Vector3(
			EntityNode.Rotation.X,
			Mathf.LerpAngle(EntityNode.Rotation.Y, targetAngle, 0.2f),
			EntityNode.Rotation.Z
		);
	}

	private static string ResolveFacingLabel(Vector2 input)
	{
		string facing;

		if (Mathf.Abs(input.Y) >= Mathf.Abs(input.X))
		{
			facing = input.Y < 0.0f ? "FRONT" : "BACK";
		}
		else
		{
			facing = input.X < 0.0f ? "LEFT" : "RIGHT";
		}

		return facing;
	}
}
