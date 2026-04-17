using Godot;
using System;

namespace BattleHarvesterStudy;

[GlobalClass]
public partial class ConeAttackBehaviorDefinition : AttackBehaviorDefinition
{
	[Export]
	public ShapeDefinition? BroadPhaseShape { get; set; }

	[Export(PropertyHint.Range, "0,360,1")]
	public float ConeAngleDegrees { get; set; } = 90.0f;

	[Export(PropertyHint.Range, "0,20,0.1")]
	public float ConeRange { get; set; } = 3.5f;

	public override void Begin(Hitbox? hitbox, SkillExecutionContext context)
	{
		if (hitbox == null)
		{
			return;
		}

		ShapeDefinition shape = BroadPhaseShape ?? new ShapeDefinition
		{
			ShapeType = HitboxShapeType.Sphere,
			Size = Vector3.One * ConeRange * 2.0f,
			Offset = Vector3.Zero,
			RotationDegrees = Vector3.Zero,
			MaxHits = 1,
			AllowRepeatHitOnSameTarget = false,
			RepeatHitInterval = 0.0f,
			DebugColor = new Color(1.0f, 0.55f, 0.15f, 0.18f),
			ShowDebugPreview = true
		};

		hitbox.ActivateShape(context, shape, hurtbox => IsTargetInsideCone(context, hurtbox));
	}

	public override void End(Hitbox? hitbox, SkillExecutionContext context)
	{
		hitbox?.DeactivateShape();
	}

	private bool IsTargetInsideCone(SkillExecutionContext context, Hurtbox hurtbox)
	{
		Node3D? target = hurtbox.GetOwner<Node3D>();
		if (target == null)
		{
			return false;
		}

		Vector3 toTarget = target.GlobalPosition - context.OriginPosition;
		toTarget.Y = 0.0f;
		float distance = toTarget.Length();
		if (distance <= 0.001f || distance > ConeRange)
		{
			return false;
		}

		Vector3 facing = context.FacingDirection;
		facing.Y = 0.0f;
		if (facing == Vector3.Zero)
		{
			facing = Vector3.Forward;
		}

		float dot = facing.Normalized().Dot(toTarget.Normalized());
		float minDot = Mathf.Cos(Mathf.DegToRad(ConeAngleDegrees * 0.5f));
		return dot >= minDot;
	}
}
