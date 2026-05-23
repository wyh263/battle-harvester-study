using Godot;

namespace BattleHarvesterStudy.Equipment;

[GlobalClass]
public partial class ArmorEquipmentDefinition : EquipmentDefinition
{
	[Export]
	public ArmorClass ArmorClass { get; set; } = ArmorClass.Medium;

	[Export(PropertyHint.Range, "0,20,1")]
	public int ArmorTier { get; set; } = 1;

	[Export(PropertyHint.Range, "0,99999,1")]
	public float MaxArmorPoint { get; set; } = 100.0f;

	[Export(PropertyHint.Range, "0,0.95,0.01")]
	public float BaseAbsorbRate { get; set; } = 0.5f;

	[Export(PropertyHint.Range, "0,1,0.01")]
	public float MovementPenalty { get; set; }

	[Export(PropertyHint.Range, "0,1,0.01")]
	public float RepairDecayMultiplier { get; set; } = 0.8f;

	[Export(PropertyHint.Range, "0,1,0.01")]
	public float StructuralDamageRatio { get; set; } = 0.05f;

	public ArmorEquipmentDefinition()
	{
		Kind = EquipmentKind.Armor;
	}
}
