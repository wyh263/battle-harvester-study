using Godot;
namespace BattleHarvesterStudy;

public partial class AttackState : State
{
	private AttackExecutor? _attackExecutor;

	public override void Init(Player entity, StateMachine machine)
	{
		base.Init(entity, machine);
		_attackExecutor = Entity.GetNodeOrNull<AttackExecutor>("Components/AttackExecutor");
	}

	public override void Enter()
	{
		base.Enter();
		Entity.DesiredMoveDirection = Vector3.Zero;
		Entity.RemoveMoveSpeedModifier(Player.RunSpeedModifierKey);
		if (_attackExecutor != null)
		{
			_attackExecutor.AttackCompleted += OnAttackCompleted;
			_attackExecutor.Play(Entity.ConsumeQueuedSkillOrDefault());
		}
		GD.Print("Enter Attack");
	}

	public override void Exit()
	{
		base.Exit();
		if (_attackExecutor != null)
		{
			_attackExecutor.AttackCompleted -= OnAttackCompleted;
			_attackExecutor.Cancel();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
	}

	private void OnAttackCompleted()
	{
		Machine.ChangeState("Idle");
	}
}
