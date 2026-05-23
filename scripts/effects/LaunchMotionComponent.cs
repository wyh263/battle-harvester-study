using Godot;

namespace BattleHarvesterStudy.Effects;

public partial class LaunchMotionComponent : Node
	, IStatusQuerySource
{
	private Node3D? _owner;
	private bool _isActive;
	private Vector3 _startPosition;
	private Vector3 _direction;
	private float _height;
	private float _distance;
	private float _duration;
	private float _elapsed;

	public override void _Ready()
	{
		_owner = GetOwner<Node3D>();
		SetPhysicsProcess(true);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_isActive || _owner == null)
		{
			return;
		}

		_elapsed += (float)delta;
		float duration = Mathf.Max(0.05f, _duration);
		float progress = Mathf.Clamp(_elapsed / duration, 0.0f, 1.0f);
		float horizontalDistance = _distance * progress;
		float verticalOffset = 4.0f * _height * progress * (1.0f - progress);

		_owner.GlobalPosition = _startPosition
			+ (_direction * horizontalDistance)
			+ (Vector3.Up * verticalOffset);

		if (progress >= 1.0f)
		{
			_owner.GlobalPosition = _startPosition + (_direction * _distance);
			_isActive = false;
		}
	}

	public void ApplyLaunch(Vector3 direction, float height, float distance, float duration)
	{
		if (_owner == null)
		{
			return;
		}

		Vector3 flattenedDirection = direction;
		flattenedDirection.Y = 0.0f;
		if (flattenedDirection == Vector3.Zero)
		{
			flattenedDirection = Vector3.Forward;
		}

		_startPosition = _owner.GlobalPosition;
		_direction = flattenedDirection.Normalized();
		_height = Mathf.Max(0.1f, height);
		_distance = Mathf.Max(0.0f, distance);
		_duration = Mathf.Max(0.05f, duration);
		_elapsed = 0.0f;
		_isActive = true;
	}

	public bool HasStatus(string statusId)
	{
		return GetStatusRemaining(statusId) > 0.0f;
	}

	public float GetStatusRemaining(string statusId)
	{
		if (!_isActive)
		{
			return 0.0f;
		}

		string normalized = statusId.Trim().ToLowerInvariant();
		if (normalized != "launch" && normalized != "launched" && normalized != "airborne")
		{
			return 0.0f;
		}

		return Mathf.Max(0.0f, _duration - _elapsed);
	}
}
