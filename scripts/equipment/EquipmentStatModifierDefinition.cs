using Godot;

namespace BattleHarvesterStudy.Equipment;

[GlobalClass]
public partial class EquipmentStatModifierDefinition : Resource
{
	[Export]
	public StatType StatType { get; set; } = StatType.AttackPower;

	[Export]
	public float Flat { get; set; }

	[Export]
	public float Multiplier { get; set; } = 1.0f;
}
