namespace BattleHarvesterStudy.Targeting;

public interface ILockTargetProvider
{
	LockAcquisitionMode Mode { get; }
	bool TryLock(CombatAimController aimController, LockRequest request);
}
