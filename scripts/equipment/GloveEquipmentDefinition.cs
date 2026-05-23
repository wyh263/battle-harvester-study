using Godot;

namespace BattleHarvesterStudy.Equipment;

[GlobalClass]
public partial class GloveEquipmentDefinition : EquipmentDefinition
{
	[Export(PropertyHint.Range, "0,99999,1")]
	public float AttackPower { get; set; } = 0.0f;

	public GloveEquipmentDefinition()
	{
		Kind = EquipmentKind.Gloves;
	}
}
