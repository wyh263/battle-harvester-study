using Godot;

namespace BattleHarvesterStudy.Targeting;

public partial class PlayerTargetingPreferences : Node
{
	[Export]
	public LockAcquisitionMode DefaultLockMode { get; set; } = LockAcquisitionMode.StrategyBased;

	[Export]
	public TargetSelectionStrategyKind DefaultLockStrategy { get; set; } = TargetSelectionStrategyKind.Nearest;
}
