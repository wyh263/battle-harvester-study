using Godot;

namespace BattleHarvesterStudy.Actors;

public partial class DummyIdleState : BattleHarvesterStudy.State.State
{
	public override void Enter()
	{
		base.Enter();
		Actor.DesiredMoveDirection = Vector3.Zero;
		Actor.RemoveMoveSpeedModifier(MovementModifierKeys.Run);
	}

	public override void _PhysicsProcess(double delta)
	{
		Actor.DesiredMoveDirection = Vector3.Zero;
		if (Actor.SkillLoadout.HasQueuedSkill)
		{
			Machine.ChangeState("Attack");
		}
	}
}
