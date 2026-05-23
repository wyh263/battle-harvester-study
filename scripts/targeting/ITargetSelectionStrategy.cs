using System.Collections.Generic;

namespace BattleHarvesterStudy.Targeting;

public interface ITargetSelectionStrategy
{
	TargetSelectionStrategyKind Kind { get; }

	List<Targetable> BuildOrderedTargets(TargetSelectionContext context);
}
