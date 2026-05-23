namespace BattleHarvesterStudy.Targeting;

public readonly struct LockRequest
{
	public LockAcquisitionMode Mode { get; }
	public TargetSelectionStrategyKind Strategy { get; }
	public Targetable? ExplicitTarget { get; }

	private LockRequest(LockAcquisitionMode mode, TargetSelectionStrategyKind strategy, Targetable? explicitTarget)
	{
		Mode = mode;
		Strategy = strategy;
		ExplicitTarget = explicitTarget;
	}

	public static LockRequest ForStrategy(TargetSelectionStrategyKind strategy)
	{
		return new LockRequest(LockAcquisitionMode.StrategyBased, strategy, null);
	}

	public static LockRequest ForExplicitTarget(LockAcquisitionMode mode, Targetable target)
	{
		return new LockRequest(mode, TargetSelectionStrategyKind.Nearest, target);
	}
}
