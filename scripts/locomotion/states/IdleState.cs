using Godot;

namespace BattleHarvesterStudy.Locomotion;

public partial class IdleState : BattleHarvesterStudy.State.State
{
	public override void Enter()
	{
		base.Enter();
		Actor.DesiredMoveDirection = Vector3.Zero;
		Actor.RemoveMoveSpeedModifier(MovementModifierKeys.Run);
		GD.Print("Enter Idle");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Actor.IsGameplayInputBlocked)
		{
			Actor.DesiredMoveDirection = Vector3.Zero;
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

		if (input != Vector2.Zero)
		{
			Machine.ChangeState("Move");
		}
	}
}
