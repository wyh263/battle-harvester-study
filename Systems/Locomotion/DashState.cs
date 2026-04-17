using Godot;
using System.Threading.Tasks;

namespace BattleHarvesterStudy;

public partial class DashState : State
{
	private int _sequenceId;
	private Vector3 _dashDirection = Vector3.Zero;

	public override void Enter()
	{
		base.Enter();

		if (!Entity.TryStartDashCooldown())
		{
			Machine.ChangeState("Idle");
			return;
		}

		_dashDirection = ResolveDashDirection();
		Entity.DesiredMoveDirection = _dashDirection;
		Entity.RemoveMoveSpeedModifier(Player.RunSpeedModifierKey);
		if (!Entity.TryStartForcedMovement(new ForcedMovementRequest
		{
			SourceKey = "dash",
			Direction = _dashDirection,
			Speed = Entity.GetDashSpeed(),
			Duration = Entity.GetDashDuration(),
			SnapVelocity = true,
			LockInput = true,
			Priority = 20
		}))
		{
			Machine.ChangeState("Idle");
			return;
		}

		Entity.SetInvulnerableFor(Entity.GetDashInvulnerableDuration());
		Entity.RememberMoveDirection(_dashDirection);

		_sequenceId++;
		_ = RunDashAsync(_sequenceId);
		GD.Print("Enter Dash");
	}

	public override void Exit()
	{
		base.Exit();
		_sequenceId++;
		Entity.DesiredMoveDirection = Vector3.Zero;
		Entity.ClearForcedMovement();
	}

	public override void _PhysicsProcess(double delta)
	{
		Entity.DesiredMoveDirection = _dashDirection;
	}

	private async Task RunDashAsync(int sequenceId)
	{
		await ToSignal(GetTree().CreateTimer(Entity.GetDashDuration()), SceneTreeTimer.SignalName.Timeout);

		if (!IsInsideTree() || Machine.CurrentState != this || _sequenceId != sequenceId)
		{
			return;
		}

		Vector2 input = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		if (input != Vector2.Zero)
		{
			Machine.ChangeState("Move");
		}
		else
		{
			Machine.ChangeState("Idle");
		}
	}

	private Vector3 ResolveDashDirection()
	{
		Vector2 input = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		if (input != Vector2.Zero)
		{
			Vector3 inputDirection = CalculateCameraRelativeDirection(input);
			if (inputDirection != Vector3.Zero)
			{
				return inputDirection;
			}
		}

		return Entity.GetDashFallbackDirection();
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

		return (right * input.X - forward * input.Y).Normalized();
	}
}
