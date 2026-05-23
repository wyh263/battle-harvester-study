using Godot;
using System.Collections.Generic;
using System.Linq;

namespace BattleHarvesterStudy.Targeting;

public sealed class NearestTargetSelectionStrategy : ITargetSelectionStrategy
{
	public TargetSelectionStrategyKind Kind => TargetSelectionStrategyKind.Nearest;

	public List<Targetable> BuildOrderedTargets(TargetSelectionContext context)
	{
		Vector3 requesterPosition = context.Requester.GlobalPosition;
		return context.Candidates
			.Where(IsValidCandidate)
			.OrderBy(target => target.GetTargetNode()!.GlobalPosition.DistanceSquaredTo(requesterPosition))
			.ToList();
	}

	private static bool IsValidCandidate(Targetable target)
	{
		return target.CanBeTargeted && target.GetTargetNode() != null;
	}
}

public sealed class FarthestTargetSelectionStrategy : ITargetSelectionStrategy
{
	public TargetSelectionStrategyKind Kind => TargetSelectionStrategyKind.Farthest;

	public List<Targetable> BuildOrderedTargets(TargetSelectionContext context)
	{
		Vector3 requesterPosition = context.Requester.GlobalPosition;
		return context.Candidates
			.Where(IsValidCandidate)
			.OrderByDescending(target => target.GetTargetNode()!.GlobalPosition.DistanceSquaredTo(requesterPosition))
			.ToList();
	}

	private static bool IsValidCandidate(Targetable target)
	{
		return target.CanBeTargeted && target.GetTargetNode() != null;
	}
}
