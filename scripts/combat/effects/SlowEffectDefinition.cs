using Godot;

namespace BattleHarvesterStudy.Combat;

[GlobalClass]
public partial class SlowEffectDefinition : EffectDefinition
{
	[Export(PropertyHint.Range, "0,1,0.01")]
	public float SlowPercent { get; set; } = 0.20f;

	[Export]
	public float DurationSeconds { get; set; } = 2.0f;

	[Export]
	public StatusPresentationDefinition? Presentation { get; set; }

	public override void Apply(HitResult hitResult)
	{
		Node3D? target = hitResult.Target.GetOwner<Node3D>();
		StatusEffectComponent? statusEffects = target?.GetNodeOrNull<StatusEffectComponent>("Components/StatusEffects");
		statusEffects?.ApplySlow(hitResult.Caster, hitResult.Skill.SkillId, SlowPercent, DurationSeconds, Presentation);
	}
}
