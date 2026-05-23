using Godot;

namespace BattleHarvesterStudy.Equipment;

[GlobalClass]
public partial class WeaponMoveSetDefinition : Resource
{
	[Export]
	public string MoveSetId { get; set; } = "moveset_generic";

	[Export]
	public string DisplayName { get; set; } = "Generic Move Set";

	[Export]
	public SkillDefinition? PrimaryAttack { get; set; }

	[Export]
	public SkillDefinition? FollowupAttack { get; set; }

	[Export]
	public SkillDefinition? FinisherAttack { get; set; }
}
