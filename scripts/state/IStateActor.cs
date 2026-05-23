using Godot;

namespace BattleHarvesterStudy.State;

public interface IStateActor
{
	Vector3 DesiredMoveDirection { get; set; }
	string FacingLabel { get; set; }
	ActorSkillLoadout SkillLoadout { get; }
	ActorSkillCooldownController SkillCooldowns { get; }
	SkillChainTracker SkillChainTracker { get; }
	bool IsGameplayInputBlocked { get; }

	bool CanStartDash();
	bool TryStartDashCooldown();
	float GetRunMultiplier();
	float GetDashSpeed();
	float GetDashDuration();
	float GetDashInvulnerableDuration();
	void SetInvulnerableFor(double duration);
	bool TryStartForcedMovement(ForcedMovementRequest request);
	void ClearForcedMovement();
	void RememberMoveDirection(Vector3 direction);
	Vector3 GetDashFallbackDirection();
	void SetMoveSpeedModifier(string key, float additive = 0.0f, float multiplier = 1.0f);
	void RemoveMoveSpeedModifier(string key);
}
