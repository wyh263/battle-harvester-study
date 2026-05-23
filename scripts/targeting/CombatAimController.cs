using Godot;
using System.Collections.Generic;
using System;

namespace BattleHarvesterStudy.Targeting;

public partial class CombatAimController : Node
{
	[Signal]
	public delegate void TargetingStateChangedEventHandler(int state);

	[Signal]
	public delegate void LockedTargetChangedEventHandler(Node3D? target, bool isLocked);

	[Export]
	public TargetSelectionStrategyKind DefaultLockStrategy { get; set; } = TargetSelectionStrategyKind.Nearest;

	public event Action<TargetingState>? TargetingStateUpdated;
	public event Action<Node3D?, bool>? LockedTargetStateChanged;

	private Node3D? _owner;
	private Targetable? _lockedTarget;
	private List<Targetable> _orderedTargets = new();
	private int _lockedTargetIndex = -1;
	private bool _hasAimOverride;
	private Vector3 _aimOverrideDirection = Vector3.Zero;

	public TargetingState CurrentState { get; private set; } = TargetingState.Free;
	public LockAcquisitionMode CurrentLockMode { get; private set; } = LockAcquisitionMode.StrategyBased;
	public TargetSelectionStrategyKind CurrentLockStrategy { get; private set; }
	public bool HasLockedTarget => TryGetLockedTarget(out _);

	public override void _Ready()
	{
		_owner = GetOwner<Node3D>();
		CurrentLockStrategy = DefaultLockStrategy;
		SetPhysicsProcess(true);
	}

	public override void _PhysicsProcess(double delta)
	{
		RefreshContinuousLock();
		ValidateLockedTarget();
	}

	public void SetLockStrategy(TargetSelectionStrategyKind strategy)
	{
		CurrentLockStrategy = strategy;
	}

	public bool TryGetAimDirection(out Vector3 direction)
	{
		direction = Vector3.Zero;
		switch (CurrentState)
		{
			case TargetingState.Locked:
				if (TryGetLockedTargetDirection(out direction))
				{
					return true;
				}

				if (_hasAimOverride && _aimOverrideDirection != Vector3.Zero)
				{
					direction = _aimOverrideDirection;
					return true;
				}

				return false;
			default:
				return false;
		}
	}

	public bool TryGetLockedTarget(out Node3D target)
	{
		target = null!;
		if (_lockedTarget == null || !GodotObject.IsInstanceValid(_lockedTarget) || !_lockedTarget.CanBeTargeted)
		{
			return false;
		}

		Node3D? targetNode = _lockedTarget.GetTargetNode();
		if (targetNode == null || !GodotObject.IsInstanceValid(targetNode))
		{
			return false;
		}

		target = targetNode;
		return true;
	}

	public bool TryGetLockedTargetDirection(out Vector3 direction)
	{
		direction = Vector3.Zero;
		if (_owner == null || !_owner.IsInsideTree())
		{
			return false;
		}

		if (_lockedTarget == null)
		{
			return false;
		}

		if (!_lockedTarget.TryGetTargetPosition(out Vector3 targetPosition))
		{
			return false;
		}

		direction = targetPosition - _owner.GlobalPosition;
		direction.Y = 0.0f;
		if (direction == Vector3.Zero)
		{
			return false;
		}

		direction = direction.Normalized();
		return true;
	}

	public bool TryLockWithStrategy(TargetSelectionStrategyKind strategy)
	{
		if (_owner == null)
		{
			return false;
		}

		List<Targetable> candidates = CollectCandidates();
		TargetSelectionContext context = new(_owner, candidates);
		List<Targetable> orderedTargets = TargetSelectionStrategies.Get(strategy).BuildOrderedTargets(context);
		if (orderedTargets.Count == 0)
		{
			return false;
		}

		CurrentLockStrategy = strategy;
		CurrentLockMode = LockAcquisitionMode.StrategyBased;
		ClearAimOverride();
		SetLockedTarget(orderedTargets[0], orderedTargets, 0);
		return true;
	}

	public void SetLockedTarget(Targetable? target)
	{
		CurrentLockMode = LockAcquisitionMode.MouseDoubleClick;
		ClearAimOverride();
		SetLockedTarget(target, target == null ? null : new List<Targetable> { target }, target == null ? -1 : 0);
	}

	public void EnterMouseFollowLock()
	{
		CurrentLockMode = LockAcquisitionMode.MouseFollow;
		_lockedTarget = null;
		_orderedTargets = new List<Targetable>();
		_lockedTargetIndex = -1;
		SetTargetingState(TargetingState.Locked);
	}

	public void SetAimOverrideDirection(Vector3 direction)
	{
		Vector3 flattened = direction;
		flattened.Y = 0.0f;
		if (flattened == Vector3.Zero)
		{
			ClearAimOverride();
			return;
		}

		_hasAimOverride = true;
		_aimOverrideDirection = flattened.Normalized();
	}

	public void EnterFreeState()
	{
		CurrentLockMode = LockAcquisitionMode.StrategyBased;
		ClearAimOverride();
		SetLockedTarget(null, null, -1);
		SetTargetingState(TargetingState.Free);
	}

	public bool CycleTarget(int step)
	{
		if (_lockedTarget == null || _orderedTargets.Count <= 1)
		{
			return false;
		}

		int count = _orderedTargets.Count;
		int nextIndex = (_lockedTargetIndex + step) % count;
		if (nextIndex < 0)
		{
			nextIndex += count;
		}

		SetLockedTarget(_orderedTargets[nextIndex], _orderedTargets, nextIndex);
		return true;
	}

	private void SetLockedTarget(Targetable? target, List<Targetable>? orderedTargets, int targetIndex)
	{
		if (_lockedTarget == target && targetIndex == _lockedTargetIndex)
		{
			return;
		}

		Node3D? previousTarget = null;
		if (_lockedTarget != null)
		{
			previousTarget = _lockedTarget.GetTargetNode();
		}

		_lockedTarget = target;
		_orderedTargets = orderedTargets ?? new List<Targetable>();
		_lockedTargetIndex = targetIndex;

		if (previousTarget != null)
		{
			LockedTargetStateChanged?.Invoke(previousTarget, false);
			EmitSignal(SignalName.LockedTargetChanged, previousTarget, false);
		}

		if (TryGetLockedTarget(out Node3D currentTarget))
		{
			SetTargetingState(TargetingState.Locked);
			LockedTargetStateChanged?.Invoke(currentTarget, true);
			EmitSignal(SignalName.LockedTargetChanged, currentTarget, true);
			return;
		}

		SetTargetingState(TargetingState.Free);
	}

	private void SetTargetingState(TargetingState state)
	{
		if (CurrentState == state)
		{
			return;
		}

		CurrentState = state;
		TargetingStateUpdated?.Invoke(CurrentState);
		EmitSignal(SignalName.TargetingStateChanged, (int)CurrentState);
	}

	private void ValidateLockedTarget()
	{
		if (_lockedTarget == null)
		{
			return;
		}

		if (!_lockedTarget.CanBeTargeted || !TryGetLockedTarget(out _))
		{
			EnterFreeState();
		}
	}

	private void RefreshContinuousLock()
	{
		if (CurrentState != TargetingState.Locked)
		{
			return;
		}

		if (CurrentLockMode != LockAcquisitionMode.StrategyBased || CurrentLockStrategy != TargetSelectionStrategyKind.Nearest)
		{
			return;
		}

		if (_owner == null)
		{
			return;
		}

		List<Targetable> candidates = CollectCandidates();
		TargetSelectionContext context = new(_owner, candidates);
		List<Targetable> orderedTargets = TargetSelectionStrategies.Get(CurrentLockStrategy).BuildOrderedTargets(context);
		if (orderedTargets.Count == 0)
		{
			EnterFreeState();
			return;
		}

		SetLockedTarget(orderedTargets[0], orderedTargets, 0);
	}

	private List<Targetable> CollectCandidates()
	{
		List<Targetable> candidates = new();
		SceneTree? tree = GetTree();
		if (tree == null)
		{
			return candidates;
		}

		foreach (Node node in tree.GetNodesInGroup(Targetable.TargetableGroupName))
		{
			if (node is not Targetable targetable)
			{
				continue;
			}

			if (!targetable.CanBeTargeted)
			{
				continue;
			}

			Node3D? targetNode = targetable.GetTargetNode();
			if (targetNode == null || targetNode == _owner)
			{
				continue;
			}

			candidates.Add(targetable);
		}

		return candidates;
	}

	private void ClearAimOverride()
	{
		_hasAimOverride = false;
		_aimOverrideDirection = Vector3.Zero;
	}
}
