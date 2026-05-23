using Godot;
using Godot.Collections;

namespace BattleHarvesterStudy.Equipment;

[GlobalClass]
public partial class EquipmentAffixDefinition : Resource
{
	[Export]
	public string AffixId { get; set; } = "affix_generic";

	[Export]
	public string DisplayName { get; set; } = "Generic Affix";

	[Export]
	public EquipmentAffixType AffixType { get; set; } = EquipmentAffixType.Prefix;

	[Export]
	public Array<EquipmentStatModifierDefinition> Modifiers { get; set; } = [];
}
