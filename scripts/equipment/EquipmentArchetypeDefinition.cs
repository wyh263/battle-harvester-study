using Godot;
using Godot.Collections;

namespace BattleHarvesterStudy.Equipment;

[GlobalClass]
public partial class EquipmentArchetypeDefinition : Resource
{
	[Export]
	public string ArchetypeId { get; set; } = "equipment_archetype";

	[Export]
	public string DisplayName { get; set; } = "Equipment Archetype";

	[Export]
	public string FamilyTag { get; set; } = "general";

	[Export]
	public Array<EquipmentSlotType> DefaultAllowedSlots { get; set; } = [];

	[Export]
	public Array<StatType> PrimaryStatTypes { get; set; } = [];

	[Export]
	public Array<string> IdentityTags { get; set; } = [];

	[Export]
	public Array<EquipmentStatRollBandDefinition> StatRollBands { get; set; } = [];

	public bool AllowsSlot(EquipmentSlotType slotType)
	{
		foreach (EquipmentSlotType allowedSlot in DefaultAllowedSlots)
		{
			if (allowedSlot == slotType)
			{
				return true;
			}
		}

		return false;
	}

}
