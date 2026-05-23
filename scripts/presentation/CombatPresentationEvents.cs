using Godot;
using System;

namespace BattleHarvesterStudy.Presentation;

public enum SkillPresentationPhase
{
	Startup,
	Active,
	Recovery,
	Completed,
	Cancelled
}

public enum ProjectilePresentationPhase
{
	Spawned,
	Hit,
	Despawned
}

public enum StatusPresentationPhase
{
	Applied,
	Expired
}

public sealed class SkillPresentationEvent
{
	public required SkillExecutionContext Context { get; init; }
	public required SkillPresentationPhase Phase { get; init; }
	public SkillPresentationDefinition? Presentation { get; init; }
}

public sealed class ProjectilePresentationEvent
{
	public required Node3D Caster { get; init; }
	public required SkillDefinition Skill { get; init; }
	public required ProjectilePresentationPhase Phase { get; init; }
	public required Vector3 WorldPosition { get; init; }
	public required Vector3 Direction { get; init; }
	public Node3D? ProjectileNode { get; init; }
	public Hurtbox? Target { get; init; }
	public ProjectilePresentationDefinition? Presentation { get; init; }
}

public sealed class HitPresentationEvent
{
	public required HitResult HitResult { get; init; }
	public SkillPresentationDefinition? Presentation { get; init; }
}

public sealed class StatusPresentationEvent
{
	public required Node3D Source { get; init; }
	public required Node3D Target { get; init; }
	public required string SkillId { get; init; }
	public required string StatusId { get; init; }
	public required StatusPresentationPhase Phase { get; init; }
	public StatusPresentationDefinition? Presentation { get; init; }
}

public static class CombatPresentationEvents
{
	public static event Action<SkillPresentationEvent>? SkillPhaseChanged;
	public static event Action<ProjectilePresentationEvent>? ProjectilePhaseChanged;
	public static event Action<HitPresentationEvent>? HitResolved;
	public static event Action<StatusPresentationEvent>? StatusPhaseChanged;

	public static void PublishSkillPhase(SkillExecutionContext context, SkillPresentationPhase phase)
	{
		SkillPhaseChanged?.Invoke(new SkillPresentationEvent
		{
			Context = context,
			Phase = phase,
			Presentation = context.Skill.Presentation
		});
	}

	public static void PublishProjectilePhase(
		Node3D caster,
		SkillDefinition skill,
		ProjectilePresentationPhase phase,
		Vector3 worldPosition,
		Vector3 direction,
		Node3D? projectileNode = null,
		Hurtbox? target = null,
		ProjectilePresentationDefinition? presentation = null)
	{
		ProjectilePhaseChanged?.Invoke(new ProjectilePresentationEvent
		{
			Caster = caster,
			Skill = skill,
			Phase = phase,
			WorldPosition = worldPosition,
			Direction = direction,
			ProjectileNode = projectileNode,
			Target = target,
			Presentation = presentation
		});
	}

	public static void PublishHitResolved(HitResult hitResult)
	{
		HitResolved?.Invoke(new HitPresentationEvent
		{
			HitResult = hitResult,
			Presentation = hitResult.Skill.Presentation
		});
	}

	public static void PublishStatusPhase(
		Node3D source,
		Node3D target,
		string skillId,
		string statusId,
		StatusPresentationPhase phase,
		StatusPresentationDefinition? presentation = null)
	{
		StatusPhaseChanged?.Invoke(new StatusPresentationEvent
		{
			Source = source,
			Target = target,
			SkillId = skillId,
			StatusId = statusId,
			Phase = phase,
			Presentation = presentation
		});
	}
}
