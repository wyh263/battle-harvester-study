using Godot;
namespace BattleHarvesterStudy.Combat;

public partial class AttackState : BattleHarvesterStudy.State.State
{
	private AttackExecutor? _attackExecutor;
	private Hurtbox? _hurtbox;

	public override void Init(Node3D entityNode, IStateActor actor, StateMachine machine)
	{
		base.Init(entityNode, actor, machine);
		_attackExecutor = EntityNode.GetNodeOrNull<AttackExecutor>("Components/AttackExecutor");
		_hurtbox = EntityNode.GetNodeOrNull<Hurtbox>("Hurtbox");
	}

	public override void Enter()
	{
		base.Enter();
		Actor.DesiredMoveDirection = Vector3.Zero;
		Actor.RemoveMoveSpeedModifier(MovementModifierKeys.Run);
		if (_attackExecutor != null)
		{
			_attackExecutor.AttackCompleted += OnAttackCompleted;
			PlayQueuedSkill();
		}

		if (_hurtbox != null)
		{
			_hurtbox.DamageTaken += OnDamageTaken;
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

		if (_hurtbox != null)
		{
			_hurtbox.DamageTaken -= OnDamageTaken;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Actor.IsGameplayInputBlocked)
		{
			return;
		}

		if (Input.IsActionJustPressed("dash")
			&& Actor.CanStartDash()
			&& Actor.SkillChainTracker.CanDashCancelCurrentSkill())
		{
			Actor.SkillLoadout.ClearQueuedSkill();
			_attackExecutor?.Cancel();
			Machine.ChangeState("Dash");
		}
	}

	private void OnAttackCompleted()
	{
		if (Actor.SkillLoadout.HasQueuedSkill)
		{
			PlayQueuedSkill();
			return;
		}

		Machine.ChangeState("Idle");
	}

	private void PlayQueuedSkill()
	{
		if (_attackExecutor == null)
		{
			return;
		}

		SkillDefinition skill = Actor.SkillLoadout.ConsumeQueuedSkillOrDefault();
		Actor.SkillCooldowns.CommitCast(skill);
		Actor.SkillChainTracker.RecordCast(skill);
		_attackExecutor.Play(skill);
	}

	private void OnDamageTaken(DamageInfo info)
	{
		if (_attackExecutor == null || !_attackExecutor.IsPlaying)
		{
			return;
		}

		if (!Actor.SkillChainTracker.CanInterruptCurrentSkill(info))
		{
			return;
		}

		Actor.SkillLoadout.ClearQueuedSkill();
		_attackExecutor.Cancel();
		Machine.ChangeState("Idle");
	}
}
