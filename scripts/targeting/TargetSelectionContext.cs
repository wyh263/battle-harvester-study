using Godot;
using System.Collections.Generic;

namespace BattleHarvesterStudy.Targeting;

public readonly struct TargetSelectionContext
{
	public TargetSelectionContext(Node3D requester, IReadOnlyList<Targetable> candidates)
	{
		Requester = requester;
		Candidates = candidates;
	}

	public Node3D Requester { get; }
	public IReadOnlyList<Targetable> Candidates { get; }
}
