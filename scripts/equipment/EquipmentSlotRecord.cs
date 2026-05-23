namespace BattleHarvesterStudy.Equipment;

public sealed class EquipmentSlotRecord
{
	public required EquipmentSlotType SlotType { get; init; }
	public required string DisplayName { get; init; }
	public required ItemCategory[] AllowedCategories { get; init; }
	public bool RequiresEquipmentDefinition { get; init; } = true;
	public bool AllowsUsableItems { get; init; }
	public string ShortcutHint { get; init; } = string.Empty;
	public ItemInstance? EquippedItem { get; set; }
}
