using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BattleHarvesterStudy.Session;

public partial class RoomGateController : Node
{
	[Signal]
	public delegate void StateChangedEventHandler(bool isOpen, bool isLocked);

	[Export]
	public NodePath EventHubPath { get; set; } = new("../RunEventHub");

	[Export]
	public Godot.Collections.Array<NodePath> BlockerPaths { get; set; } = [];

	[Export]
	public string ScopeId { get; set; } = string.Empty;

	[Export]
	public Godot.Collections.Array<string> OpenOnEvents { get; set; } = [];

	[Export]
	public Godot.Collections.Array<string> CloseOnEvents { get; set; } = [];

	[Export]
	public Godot.Collections.Array<string> LockOnEvents { get; set; } = [];

	[Export]
	public Godot.Collections.Array<string> UnlockOnEvents { get; set; } = [];

	[Export]
	public bool StartOpen { get; set; } = true;

	[Export]
	public bool StartLocked { get; set; }

	public bool IsOpen { get; private set; }
	public bool IsLocked { get; private set; }

	private RunEventHub? _eventHub;
	private readonly List<Node3D> _blockers = [];

	public override void _Ready()
	{
		foreach (NodePath blockerPath in BlockerPaths)
		{
			Node3D? blocker = GetNodeOrNull<Node3D>(blockerPath);
			if (blocker != null)
			{
				_blockers.Add(blocker);
			}
		}

		_eventHub = GetNodeOrNull<RunEventHub>(EventHubPath);
		if (_eventHub != null)
		{
			_eventHub.EventRaised += OnEventRaised;
		}

		IsOpen = StartOpen;
		IsLocked = StartLocked;
		ApplyState();
	}

	public override void _ExitTree()
	{
		if (_eventHub != null)
		{
			_eventHub.EventRaised -= OnEventRaised;
		}
	}

	public void Open()
	{
		if (IsOpen)
		{
			return;
		}

		IsOpen = true;
		ApplyState();
	}

	public void Close()
	{
		if (!IsOpen)
		{
			return;
		}

		IsOpen = false;
		ApplyState();
	}

	public void Lock()
	{
		if (IsLocked)
		{
			return;
		}

		IsLocked = true;
		ApplyState();
	}

	public void Unlock()
	{
		if (!IsLocked)
		{
			return;
		}

		IsLocked = false;
		ApplyState();
	}

	private void OnEventRaised(string eventId, string eventScopeId)
	{
		if (!string.IsNullOrWhiteSpace(eventScopeId) &&
			!string.Equals(eventScopeId, ScopeId, System.StringComparison.Ordinal))
		{
			return;
		}

		if (OpenOnEvents.Contains(eventId))
		{
			Open();
		}

		if (CloseOnEvents.Contains(eventId))
		{
			Close();
		}

		if (UnlockOnEvents.Contains(eventId))
		{
			Unlock();
		}

		if (LockOnEvents.Contains(eventId))
		{
			Lock();
		}
	}

	private void ApplyState()
	{
		bool blockerEnabled = !IsOpen;
		foreach (Node3D blocker in _blockers)
		{
			blocker.Visible = blockerEnabled;
			foreach (CollisionShape3D collisionShape in blocker.FindChildren("*", "CollisionShape3D", true, false).OfType<CollisionShape3D>())
			{
				collisionShape.Disabled = !blockerEnabled;
			}
		}

		EmitSignal(SignalName.StateChanged, IsOpen, IsLocked);
	}
}
