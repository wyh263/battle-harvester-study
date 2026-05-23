using Godot;

namespace BattleHarvesterStudy.Combat;

[GlobalClass]
public partial class BleedEffectDefinition : EffectDefinition
{
	[Export]
	public int DamagePerTick { get; set; } = 10;

	[Export]
	public float TickIntervalSeconds { get; set; } = 1.0f;

	[Export]
	public float TotalDurationSeconds { get; set; } = 5.0f;

	[Export]
	public StatusPresentationDefinition? Presentation { get; set; }

	public override void Apply(HitResult hitResult)
	{
		Node3D? target = hitResult.Target.GetOwner<Node3D>();
		StatusEffectComponent? statusEffects = target?.GetNodeOrNull<StatusEffectComponent>("Components/StatusEffects");
		statusEffects?.ApplyBleed(hitResult.Caster, hitResult.Skill.SkillId, DamagePerTick, TickIntervalSeconds, TotalDurationSeconds, Presentation);
	}
}
