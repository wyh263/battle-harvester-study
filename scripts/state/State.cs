using Godot;

namespace BattleHarvesterStudy;

public abstract partial class State : Node
{
	protected Player Entity { get; private set; } = null!;
	protected StateMachine Machine { get; private set; } = null!;

	public virtual void Init(Player entity, StateMachine machine)
	{
		Entity = entity;
		Machine = machine;
		SetPhysicsProcess(false);
	}

	public virtual void Enter()
	{
		SetPhysicsProcess(true);
	}

	public virtual void Exit()
	{
		SetPhysicsProcess(false);
	}
}
