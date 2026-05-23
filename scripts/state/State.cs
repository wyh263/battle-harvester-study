using Godot;

namespace BattleHarvesterStudy.State;

public abstract partial class State : Node
{
	protected Node3D EntityNode { get; private set; } = null!;
	protected IStateActor Actor { get; private set; } = null!;
	protected StateMachine Machine { get; private set; } = null!;

	public virtual void Init(Node3D entityNode, IStateActor actor, StateMachine machine)
	{
		EntityNode = entityNode;
		Actor = actor;
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
