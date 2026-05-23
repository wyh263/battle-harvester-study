using Godot;

namespace BattleHarvesterStudy.Locomotion;

public partial class ForcedMovementComponent : Node
{
	private bool _hasActiveRequest;
	private ForcedMovementRequest _currentRequest;
	private double _remainingTime;

	public bool HasActiveRequest => _hasActiveRequest;
	public Vector3 CurrentDirection => _hasActiveRequest ? _currentRequest.Direction : Vector3.Zero;
	public float CurrentSpeed => _hasActiveRequest ? _currentRequest.Speed : 0.0f;
	public bool CurrentSnapVelocity => _hasActiveRequest && _currentRequest.SnapVelocity;
	public bool CurrentLocksInput => _hasActiveRequest && _currentRequest.LockInput;
	public string CurrentSourceKey => _hasActiveRequest ? _currentRequest.SourceKey : "";

	public bool TryStart(ForcedMovementRequest request)
	{
		if (request.Direction == Vector3.Zero || request.Duration <= 0.0f || request.Speed <= 0.0f)
		{
			return false;
		}

		request = request with
		{
			Direction = request.Direction.Normalized()
		};

		if (!_hasActiveRequest || request.Priority >= _currentRequest.Priority)
		{
			_currentRequest = request;
			_remainingTime = request.Duration;
			_hasActiveRequest = true;
			return true;
		}

		return false;
	}

	public void Advance(double delta)
	{
		if (!_hasActiveRequest)
		{
			return;
		}

		_remainingTime -= delta;
		if (_remainingTime <= 0.0)
		{
			Clear();
		}
	}

	public void Clear()
	{
		_hasActiveRequest = false;
		_currentRequest = default;
		_remainingTime = 0.0;
	}
}
