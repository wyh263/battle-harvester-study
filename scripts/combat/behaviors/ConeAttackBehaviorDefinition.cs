using Godot;
using System;

namespace BattleHarvesterStudy.Combat;

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

		ShapeDefinition shape = BuildBroadPhaseShape();

		hitbox.ActivateShape(context, shape, hurtbox => IsTargetInsideCone(context, hurtbox));
	}

	public override void End(Hitbox? hitbox, SkillExecutionContext context)
	{
		hitbox?.DeactivateShape();
	}

	private ShapeDefinition BuildBroadPhaseShape()
	{
		ShapeDefinition template = BroadPhaseShape ?? new ShapeDefinition
		{
			ShapeType = HitboxShapeType.Box,
			Size = new Vector3(1.0f, 0.9f, ConeRange),
			Offset = new Vector3(0.0f, 0.45f, -ConeRange * 0.5f),
			RotationDegrees = Vector3.Zero,
			MaxHits = 1,
			AllowRepeatHitOnSameTarget = false,
			RepeatHitInterval = 0.0f,
			DebugColor = new Color(1.0f, 0.55f, 0.15f, 0.18f),
			ShowDebugPreview = true
		};

		float halfAngleRadians = Mathf.DegToRad(ConeAngleDegrees * 0.5f);
		float width = Mathf.Max(0.8f, Mathf.Tan(halfAngleRadians) * ConeRange * 2.0f);
		float height = Mathf.Max(0.6f, template.Size.Y);

		return new ShapeDefinition
		{
			ShapeType = HitboxShapeType.Box,
			Size = new Vector3(width, height, ConeRange),
			Offset = new Vector3(0.0f, template.Offset.Y, -ConeRange * 0.5f),
			RotationDegrees = Vector3.Zero,
			MaxHits = template.MaxHits,
			AllowRepeatHitOnSameTarget = template.AllowRepeatHitOnSameTarget,
			RepeatHitInterval = template.RepeatHitInterval,
			DebugColor = template.DebugColor,
			ShowDebugPreview = template.ShowDebugPreview
		};
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
