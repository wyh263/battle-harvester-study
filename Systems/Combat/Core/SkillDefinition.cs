using Godot;
using Godot.Collections;

namespace BattleHarvesterStudy;

[GlobalClass]
public partial class SkillDefinition : Resource
{
	[Export]
	public string SkillId { get; set; } = "basic_attack";

	[Export]
	public string DisplayName { get; set; } = "Basic Attack";

	[Export]
	public string StartupLabel { get; set; } = "START";

	[Export]
	public string ActiveLabel { get; set; } = "ACTIVE";

	[Export]
	public string RecoveryLabel { get; set; } = "RECOVERY";

	[Export(PropertyHint.Range, "0,5,0.01")]
	public float StartupSeconds { get; set; } = 0.15f;

	[Export(PropertyHint.Range, "0,5,0.01")]
	public float ActiveSeconds { get; set; } = 0.12f;

	[Export(PropertyHint.Range, "0,5,0.01")]
	public float RecoverySeconds { get; set; } = 0.20f;

	[Export]
	public AttackBehaviorDefinition? AttackBehavior { get; set; }

	[Export]
	public Array<EffectDefinition> Effects { get; set; } = new();
}
