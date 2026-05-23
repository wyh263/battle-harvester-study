using Godot;
using System.Collections.Generic;

namespace BattleHarvesterStudy.State;

public partial class StateMachine : Node
{
	[Export]
	public NodePath InitialStatePath = new("Idle");

	private readonly Dictionary<string, State> _states = new();

	public State? CurrentState { get; private set; }

	public override void _Ready()
	{
		Node3D? entityNode = GetOwner<Node3D>();
		if (entityNode is not IStateActor actor)
		{
			return;
		}

		foreach (Node child in GetChildren())
		{
			if (child is not State state)
			{
				continue;
			}

			_states[state.Name] = state;
			state.Init(entityNode, actor, this);
		}

		if (InitialStatePath.IsEmpty)
		{
			return;
		}

		ChangeState(InitialStatePath);
	}

	public bool ChangeState(NodePath nextStatePath)
	{
		string stateName = nextStatePath.ToString();
		return ChangeState(stateName);
	}

	public bool ChangeState(string nextStateName)
	{
		if (!_states.TryGetValue(nextStateName, out State? nextState))
		{
			return false;
		}

		if (CurrentState == nextState)
		{
			return true;
		}

		CurrentState?.Exit();
		CurrentState = nextState;
		CurrentState.Enter();

		GD.Print($"[FSM] Switch -> {CurrentState.Name}");
		return true;
	}
}
