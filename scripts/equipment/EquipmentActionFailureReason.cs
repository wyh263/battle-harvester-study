namespace BattleHarvesterStudy.Equipment;

public enum EquipmentActionFailureReason
{
	None = 0,
	MissingSlot = 1,
	MissingEquipmentDefinition = 2,
	SlotNotAllowed = 3,
	CategoryNotAllowed = 4,
	SlotOccupied = 5,
	SecondaryWeaponOccupied = 6,
	SlotEmpty = 7,
	TargetContainerRejected = 8,
}
