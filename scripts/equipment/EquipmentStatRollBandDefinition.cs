using Godot;

namespace BattleHarvesterStudy.Equipment;

[GlobalClass]
public partial class EquipmentStatRollBandDefinition : Resource
{
	[Export]
	public StatType StatType { get; set; } = StatType.AttackPower;

	[Export]
	public float MinFlat { get; set; }

	[Export]
	public float MaxFlat { get; set; }

	[Export]
	public float MinMultiplier { get; set; } = 1.0f;

	[Export]
	public float MaxMultiplier { get; set; } = 1.0f;
}
