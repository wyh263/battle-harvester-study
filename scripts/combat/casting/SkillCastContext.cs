using Godot;

namespace BattleHarvesterStudy.Combat;

public sealed class SkillCastContext
{
	public SkillCastContext(
		Node3D caster,
		SkillDefinition skill,
		ActorSkillCooldownController cooldowns,
		ActorSkillResourceController? resources,
		CombatAimController? aimController,
		SkillChainTracker? chainTracker)
	{
		Caster = caster;
		Skill = skill;
		Cooldowns = cooldowns;
		Resources = resources;
		AimController = aimController;
		ChainTracker = chainTracker;

		if (AimController != null && AimController.TryGetLockedTarget(out Node3D lockedTarget))
		{
			LockedTargetNode = lockedTarget;
		}
	}

	public Node3D Caster { get; }
	public SkillDefinition Skill { get; }
	public ActorSkillCooldownController Cooldowns { get; }
	public ActorSkillResourceController? Resources { get; }
	public CombatAimController? AimController { get; }
	public SkillChainTracker? ChainTracker { get; }
	public Node3D? LockedTargetNode { get; }
	public bool HasLockedTarget => LockedTargetNode != null;

	public bool TargetHasStatus(string statusId)
	{
		return LockedTargetNode != null && StatusQueryUtility.HasStatus(LockedTargetNode, statusId);
	}

	public float GetTargetStatusRemaining(string statusId)
	{
		return LockedTargetNode == null ? 0.0f : StatusQueryUtility.GetStatusRemaining(LockedTargetNode, statusId);
	}
}
