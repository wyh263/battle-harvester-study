namespace BattleHarvesterStudy.Targeting;

public sealed class StrategyLockTargetProvider : ILockTargetProvider
{
	public LockAcquisitionMode Mode => LockAcquisitionMode.StrategyBased;

	public bool TryLock(CombatAimController aimController, LockRequest request)
	{
		return aimController.TryLockWithStrategy(request.Strategy);
	}
}

public sealed class MouseDoubleClickLockTargetProvider : ILockTargetProvider
{
	public LockAcquisitionMode Mode => LockAcquisitionMode.MouseDoubleClick;

	public bool TryLock(CombatAimController aimController, LockRequest request)
	{
		if (request.ExplicitTarget == null)
		{
			return false;
		}

		aimController.SetLockedTarget(request.ExplicitTarget);
		return aimController.HasLockedTarget;
	}
}

public sealed class MouseFollowLockTargetProvider : ILockTargetProvider
{
	public LockAcquisitionMode Mode => LockAcquisitionMode.MouseFollow;

	public bool TryLock(CombatAimController aimController, LockRequest request)
	{
		aimController.EnterMouseFollowLock();
		return aimController.CurrentState == TargetingState.Locked;
	}
}
