namespace BattleHarvesterStudy.Inventory;

public sealed class ContainerInteractionEvent
{
	public required WorldContainer Container { get; init; }
	public required bool Succeeded { get; init; }
	public required ContainerAccessCheckResult AccessResult { get; init; }
}
