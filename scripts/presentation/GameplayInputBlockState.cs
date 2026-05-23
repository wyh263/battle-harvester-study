namespace BattleHarvesterStudy.Presentation;

public readonly record struct GameplayInputBlockState(
	bool MovementBlocked,
	bool CombatBlocked,
	bool TargetingBlocked,
	bool WorldInteractionBlocked,
	bool CameraBlocked)
{
	public bool AnyBlocked =>
		MovementBlocked
		|| CombatBlocked
		|| TargetingBlocked
		|| WorldInteractionBlocked
		|| CameraBlocked;

	public static GameplayInputBlockState None => new(
		MovementBlocked: false,
		CombatBlocked: false,
		TargetingBlocked: false,
		WorldInteractionBlocked: false,
		CameraBlocked: false);
}
