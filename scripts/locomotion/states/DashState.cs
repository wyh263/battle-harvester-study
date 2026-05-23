using Godot;
using System.Threading.Tasks;

namespace BattleHarvesterStudy.Locomotion;

public partial class DashState : BattleHarvesterStudy.State.State
{
	private int _sequenceId;
	private Vector3 _dashDirection = Vector3.Zero;

	public override void Enter()
	{
		base.Enter();

		if (!Actor.TryStartDashCooldown())
		{
			Machine.ChangeState("Idle");
			return;
		}

		_dashDirection = ResolveDashDirection();
		Actor.DesiredMoveDirection = _dashDirection;
		Actor.RemoveMoveSpeedModifier(MovementModifierKeys.Run);
		if (!Actor.TryStartForcedMovement(new ForcedMovementRequest
		{
			SourceKey = "dash",
			Direction = _dashDirection,
			Speed = Actor.GetDashSpeed(),
			Duration = Actor.GetDashDuration(),
			SnapVelocity = true,
			LockInput = true,
			Priority = 20
		}))
		{
			Machine.ChangeState("Idle");
			return;
		}

		Actor.SetInvulnerableFor(Actor.GetDashInvulnerableDuration());
		Actor.RememberMoveDirection(_dashDirection);

		_sequenceId++;
		_ = RunDashAsync(_sequenceId);
		GD.Print("Enter Dash");
	}

	public override void Exit()
	{
		base.Exit();
		_sequenceId++;
		Actor.DesiredMoveDirection = Vector3.Zero;
		Actor.ClearForcedMovement();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Actor.IsGameplayInputBlocked)
		{
			return;
		}

		Actor.DesiredMoveDirection = _dashDirection;
	}

	private async Task RunDashAsync(int sequenceId)
	{
		await ToSignal(GetTree().CreateTimer(Actor.GetDashDuration()), SceneTreeTimer.SignalName.Timeout);

		if (!IsInsideTree() || Machine.CurrentState != this || _sequenceId != sequenceId)
		{
			return;
		}

		if (Actor.IsGameplayInputBlocked)
		{
			Machine.ChangeState("Idle");
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

		return Actor.GetDashFallbackDirection();
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
