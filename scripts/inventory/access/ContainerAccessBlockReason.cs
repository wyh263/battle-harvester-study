namespace BattleHarvesterStudy.Inventory;

public enum ContainerAccessBlockReason
{
	None = 0,
	MissingRequester = 1,
	Locked = 2,
	OutOfRange = 3,
	MissingRequiredTag = 4,
	SingleUseConsumed = 5,
}
