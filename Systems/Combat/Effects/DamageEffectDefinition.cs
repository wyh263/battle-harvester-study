using Godot;

namespace BattleHarvesterStudy;

[GlobalClass]
public partial class DamageEffectDefinition : EffectDefinition
{
	[Export]
	public int Damage { get; set; } = 10;

	[Export]
	public float KnockbackForce { get; set; } = 2.0f;

	[Export]
	public bool CausesForcedMovement { get; set; } = true;

	[Export]
	public float ForcedMovementDuration { get; set; } = 0.10f;

	[Export]
	public int ForcedMovementPriority { get; set; } = 10;

	public override void Apply(HitResult hitResult)
	{
		Vector3 knockback = hitResult.HitDirection * KnockbackForce;
		hitResult.Target.TakeDamage(new DamageInfo(
			Damage,
			knockback,
			hitResult.Caster,
			hitResult.Skill.SkillId,
			CausesForcedMovement,
			ForcedMovementDuration,
			ForcedMovementPriority
		));
	}
}
