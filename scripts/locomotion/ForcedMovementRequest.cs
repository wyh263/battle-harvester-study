using Godot;

namespace BattleHarvesterStudy.Locomotion;

public readonly struct ForcedMovementRequest
{
	public string SourceKey { get; init; }
	public Vector3 Direction { get; init; }
	public float Speed { get; init; }
	public float Duration { get; init; }
	public bool SnapVelocity { get; init; }
	public bool LockInput { get; init; }
	public int Priority { get; init; }
}
