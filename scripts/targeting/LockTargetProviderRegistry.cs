using System.Collections.Generic;

namespace BattleHarvesterStudy.Targeting;

public static class LockTargetProviderRegistry
{
	private static readonly Dictionary<LockAcquisitionMode, ILockTargetProvider> Providers = new()
	{
		{ LockAcquisitionMode.StrategyBased, new StrategyLockTargetProvider() },
		{ LockAcquisitionMode.MouseDoubleClick, new MouseDoubleClickLockTargetProvider() },
		{ LockAcquisitionMode.MouseFollow, new MouseFollowLockTargetProvider() }
	};

	public static ILockTargetProvider Get(LockAcquisitionMode mode)
	{
		return Providers[mode];
	}
}
