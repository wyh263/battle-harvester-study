using Godot;

namespace BattleHarvesterStudy;

public partial class MoveState : State
{
	private Label3D? _directionLabel;

	public override void Init(Player entity, StateMachine machine)
	{
		base.Init(entity, machine);
		_directionLabel = Entity.GetNodeOrNull<Label3D>("Visuals/DirectionLabel");
	}

	public override void Enter()
	{
		base.Enter();
		Entity.DesiredMoveDirection = Vector3.Zero;
		GD.Print("Enter Move");
	}

	public override void Exit()
	{
		base.Exit();
		Entity.RemoveMoveSpeedModifier(Player.RunSpeedModifierKey);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionJustPressed("dash") && Entity.CanStartDash())
		{
			Machine.ChangeState("Dash");
			return;
		}

		if (Input.IsActionJustPressed("attack"))
		{
			Entity.QueueSkill(Entity.GetBasicSkillDefinition());
			Machine.ChangeState("Attack");
			return;
		}

		if (Input.IsActionJustPressed("skill_u"))
		{
			Entity.QueueSkill(Entity.GetLaserSkillDefinition());
			Machine.ChangeState("Attack");
			return;
		}

		if (Input.IsActionJustPressed("skill_i"))
		{
			Entity.QueueSkill(Entity.GetWideBleedSlashSkillDefinition());
			Machine.ChangeState("Attack");
			return;
		}

		if (Input.IsActionJustPressed("skill_o"))
		{
			Entity.QueueSkill(Entity.GetSlowOrbSkillDefinition());
			Machine.ChangeState("Attack");
			return;
		}

		Vector2 input = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		Vector3 direction = CalculateCameraRelativeDirection(input);
		Entity.DesiredMoveDirection = direction;

		if (Input.IsActionPressed("run"))
		{
			Entity.SetMoveSpeedModifier(Player.RunSpeedModifierKey, multiplier: Entity.GetRunMultiplier());
		}
		else
		{
			Entity.RemoveMoveSpeedModifier(Player.RunSpeedModifierKey);
		}

		if (direction != Vector3.Zero)
		{
			Entity.RememberMoveDirection(direction);
			RotateTowardMovement(direction);
			UpdateDirectionLabel(input);
		}
		else
		{
			Entity.DesiredMoveDirection = Vector3.Zero;
			Entity.RemoveMoveSpeedModifier(Player.RunSpeedModifierKey);
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

		Entity.Rotation = new Vector3(
			Entity.Rotation.X,
			Mathf.LerpAngle(Entity.Rotation.Y, targetAngle, 0.2f),
			Entity.Rotation.Z
		);
	}

	private void UpdateDirectionLabel(Vector2 input)
	{
		if (_directionLabel == null || input == Vector2.Zero)
		{
			return;
		}

		string facing;

		if (Mathf.Abs(input.Y) >= Mathf.Abs(input.X))
		{
			facing = input.Y < 0.0f ? "FRONT" : "BACK";
		}
		else
		{
			facing = input.X < 0.0f ? "LEFT" : "RIGHT";
		}

		Entity.FacingLabel = facing;
		_directionLabel.Text = facing;
	}
}
