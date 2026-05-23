using Godot;

namespace BattleHarvesterStudy.Combat;

[GlobalClass]
public partial class ProjectileAttackBehaviorDefinition : AttackBehaviorDefinition
{
	[Export]
	public ShapeDefinition? ProjectileShape { get; set; }

	[Export(PropertyHint.Range, "0,50,0.1")]
	public float Speed { get; set; } = 14.0f;

	[Export(PropertyHint.Range, "0,50,0.1")]
	public float MaxDistance { get; set; } = 8.0f;

	[Export]
	public bool DestroyOnFirstHit { get; set; } = true;

	public override void Begin(Hitbox? hitbox, SkillExecutionContext context)
	{
		CombatResolver? resolver = context.Caster.GetNodeOrNull<CombatResolver>("Components/CombatResolver");
		SceneTree? tree = context.Caster.GetTree();
		if (resolver == null || tree?.CurrentScene == null)
		{
			return;
		}

		CombatProjectile projectile = new();
		projectile.Initialize(
			context,
			resolver,
			ProjectileShape ?? CreateDefaultProjectileShape(),
			Speed,
			MaxDistance,
			DestroyOnFirstHit,
			context.Skill.Presentation?.ProjectilePresentation
		);
		tree.CurrentScene.AddChild(projectile);
	}

	public override void End(Hitbox? hitbox, SkillExecutionContext context)
	{
	}

	private static ShapeDefinition CreateDefaultProjectileShape()
	{
		return new ShapeDefinition
		{
			ShapeType = HitboxShapeType.Sphere,
			Size = Vector3.One * 0.8f,
			Offset = Vector3.Zero,
			RotationDegrees = Vector3.Zero,
			MaxHits = 1,
			AllowRepeatHitOnSameTarget = false,
			RepeatHitInterval = 0.0f,
			DebugColor = new Color(0.3f, 0.9f, 1.0f, 0.35f),
			ShowDebugPreview = true
		};
	}
}
