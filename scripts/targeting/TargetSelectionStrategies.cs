using System.Collections.Generic;

namespace BattleHarvesterStudy.Targeting;

public static class TargetSelectionStrategies
{
	private static readonly Dictionary<TargetSelectionStrategyKind, ITargetSelectionStrategy> Strategies = new()
	{
		{ TargetSelectionStrategyKind.Nearest, new NearestTargetSelectionStrategy() },
		{ TargetSelectionStrategyKind.Farthest, new FarthestTargetSelectionStrategy() }
	};

	public static ITargetSelectionStrategy Get(TargetSelectionStrategyKind kind)
	{
		return Strategies[kind];
	}
}
