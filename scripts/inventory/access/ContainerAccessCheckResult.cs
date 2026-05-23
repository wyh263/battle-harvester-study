using Godot;
using Godot.Collections;

namespace BattleHarvesterStudy.Inventory;

public readonly struct ContainerAccessCheckResult
{
	public bool CanAccess { get; init; }
	public ContainerAccessBlockReason BlockReason { get; init; }
	public float RequiredDistance { get; init; }
	public float CurrentDistance { get; init; }
	public string FailureTextKey { get; init; }
	public Dictionary<string, Variant> FailureFormatArgs { get; init; }
}
