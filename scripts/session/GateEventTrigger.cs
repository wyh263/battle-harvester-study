using Godot;

namespace BattleHarvesterStudy.Session;

public partial class GateEventTrigger : Area3D
{
	[Export]
	public NodePath EventHubPath { get; set; } = new("../../RunEventHub");

	[Export]
	public NodePath PlayerPath { get; set; } = new("../../Player");

	[Export]
	public string ScopeId { get; set; } = string.Empty;

	[Export]
	public string OpenAction { get; set; } = "interact";

	[Export]
	public string CloseAction { get; set; } = "lock_exit";

	[Export]
	public string OpenEventId { get; set; } = string.Empty;

	[Export]
	public string CloseEventId { get; set; } = string.Empty;

	private RunEventHub? _eventHub;
	private Node3D? _player;
	private int _playerOverlapCount;

	public override void _Ready()
	{
		_eventHub = GetNodeOrNull<RunEventHub>(EventHubPath);
		_player = GetNodeOrNull<Node3D>(PlayerPath);
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
	}

	public override void _ExitTree()
	{
		BodyEntered -= OnBodyEntered;
		BodyExited -= OnBodyExited;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (_playerOverlapCount <= 0 || _eventHub == null)
		{
			return;
		}

		if (@event.IsActionPressed(OpenAction) && !string.IsNullOrWhiteSpace(OpenEventId))
		{
			_eventHub.RaiseEvent(OpenEventId, ScopeId);
			GetViewport().SetInputAsHandled();
			return;
		}

		if (@event.IsActionPressed(CloseAction) && !string.IsNullOrWhiteSpace(CloseEventId))
		{
			_eventHub.RaiseEvent(CloseEventId, ScopeId);
			GetViewport().SetInputAsHandled();
		}
	}

	private void OnBodyEntered(Node3D body)
	{
		if (IsPlayerBody(body))
		{
			_playerOverlapCount++;
		}
	}

	private void OnBodyExited(Node3D body)
	{
		if (IsPlayerBody(body) && _playerOverlapCount > 0)
		{
			_playerOverlapCount--;
		}
	}

	private bool IsPlayerBody(Node body)
	{
		return _player != null && (body == _player || body.GetOwner() == _player);
	}
}
