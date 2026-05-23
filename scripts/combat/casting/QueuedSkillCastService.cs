using Godot;
using BattleHarvesterStudy.Equipment;

namespace BattleHarvesterStudy.Combat;

public static class QueuedSkillCastService
{
	public static bool TryQueueSkill(
		Node3D caster,
		SkillDefinition skill,
		ActorSkillLoadout loadout,
		ActorSkillCooldownController cooldowns,
		CombatAimController? aimController,
		SkillChainTracker? chainTracker,
		out SkillCastCheckResult castCheckResult)
	{
		EquipmentComponent? equipment = caster.GetNodeOrNull<EquipmentComponent>("Components/Equipment");
		WeaponEquipmentDefinition? activeWeapon = equipment?.GetActiveWeaponDefinition();
		if (skill.LoadoutCategory == SkillLoadoutCategory.Weapon && !skill.SupportsWeapon(activeWeapon))
		{
			castCheckResult = new SkillCastCheckResult
			{
				CanCast = false,
				BlockReason = SkillCastBlockReason.RequirementNotMet,
				FailureDetail = "WEAPON INCOMPATIBLE"
			};
			return false;
		}

		castCheckResult = cooldowns.CheckCast(skill);
		if (!castCheckResult.CanCast)
		{
			return false;
		}

		if (chainTracker != null && chainTracker.IsSkillActive && !chainTracker.CanBufferNextSkill())
		{
			castCheckResult = new SkillCastCheckResult
			{
				CanCast = false,
				BlockReason = SkillCastBlockReason.ChainWindowClosed,
				RemainingSkillCooldown = castCheckResult.RemainingSkillCooldown,
				RemainingGroupCooldown = castCheckResult.RemainingGroupCooldown,
				RemainingGlobalCooldown = castCheckResult.RemainingGlobalCooldown,
				CurrentResource = castCheckResult.CurrentResource,
				RequiredResource = castCheckResult.RequiredResource,
				FailureDetail = chainTracker.GetBufferBlockDetail()
			};
			return false;
		}

		SkillCastContext context = new(caster, skill, cooldowns, null, aimController, chainTracker);
		foreach (SkillCastRequirement requirement in skill.CastRequirements)
		{
			SkillRequirementCheckResult requirementResult = requirement.Evaluate(context);
			if (requirementResult.Satisfied)
			{
				continue;
			}

			castCheckResult = new SkillCastCheckResult
			{
				CanCast = false,
				BlockReason = SkillCastBlockReason.RequirementNotMet,
				RemainingSkillCooldown = castCheckResult.RemainingSkillCooldown,
				RemainingGroupCooldown = castCheckResult.RemainingGroupCooldown,
				RemainingGlobalCooldown = castCheckResult.RemainingGlobalCooldown,
				CurrentResource = castCheckResult.CurrentResource,
				RequiredResource = castCheckResult.RequiredResource,
				FailureDetail = requirementResult.FailureDetail
			};
			return false;
		}

		loadout.QueueSkill(skill);
		if (skill.LoadoutCategory == SkillLoadoutCategory.Weapon)
		{
			equipment?.NotifyActiveWeaponSkillCast(skill);
		}

		castCheckResult = new SkillCastCheckResult
		{
			CanCast = true,
			BlockReason = SkillCastBlockReason.None,
			FailureDetail = string.Empty
		};
		return true;
	}
}
