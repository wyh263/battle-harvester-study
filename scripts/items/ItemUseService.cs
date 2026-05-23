using Godot;
using BattleHarvesterStudy.Attributes;
using BattleHarvesterStudy.Combat;
using BattleHarvesterStudy.Inventory;
using BattleHarvesterStudy.Inventory.Search;

namespace BattleHarvesterStudy.Items;

public static class ItemUseService
{
	public static bool TryApplyItemEffects(
		ItemInstance? sourceItem,
		ItemDefinition definition,
		EquipmentComponent? equipment,
		HealthComponent? health,
		ActorSkillResourceController? resources,
		Node3D? caster,
		ActorSkillLoadout? skillLoadout,
		ActorSkillCooldownController? skillCooldowns,
		CombatAimController? aimController,
		SkillChainTracker? chainTracker,
		out ItemUseFailureReason failureReason)
	{
		failureReason = ItemUseFailureReason.None;
		UsableItemDefinition? usable = definition.Usable;
		WeaponSkillItemDefinition? weaponSkill = definition.WeaponSkill;
		if (usable == null && weaponSkill == null)
		{
			failureReason = ItemUseFailureReason.NotUsable;
			return false;
		}

		bool appliedAnyEffect = false;
		if (weaponSkill != null)
		{
			if (equipment?.TryInstallWeaponSkillOnActiveWeapon(
					weaponSkill,
					sourceItem?.Definition,
					sourceItem?.AcquisitionState ?? ItemAcquisitionState.Base,
					sourceItem?.RemainingUses ?? weaponSkill.GrantedUses,
					out _) == true)
			{
				appliedAnyEffect = true;
			}
			else if (usable == null)
			{
				failureReason = ItemUseFailureReason.WeaponSkillInstallBlocked;
				return false;
			}
		}

		if (usable != null && health != null && usable.RestoreHealth > 0.0f)
		{
			bool canRestoreHealth = usable.AllowUseAtFull || health.CurrentHealth + 0.0001f < health.MaxHealth;
			if (canRestoreHealth)
			{
				health.Restore(usable.RestoreHealth);
				appliedAnyEffect = true;
			}
		}

		if (usable != null && resources != null && usable.RestoreResource > 0.0f)
		{
			bool canRestoreResource = usable.AllowUseAtFull || resources.CurrentResource + 0.0001f < resources.MaxResource;
			if (canRestoreResource)
			{
				resources.Restore(usable.RestoreResource);
				appliedAnyEffect = true;
			}
		}

		if (usable != null && usable.RepairsArmor)
		{
			ArmorComponent? armor = caster?.GetNodeOrNull<ArmorComponent>("Components/Armor");
			if (armor != null && armor.TryRepair(usable.RepairTier))
			{
				appliedAnyEffect = true;
			}
			else if (!appliedAnyEffect)
			{
				failureReason = ItemUseFailureReason.RepairBlocked;
				return false;
			}
		}

		if (usable?.ContainerModifierProfile != null)
		{
			ActiveContainerModifierComponent? activeModifiers = caster?.GetNodeOrNull<ActiveContainerModifierComponent>("Components/ActiveContainerModifiers");
			if (activeModifiers?.ApplyProfile(usable.ContainerModifierProfile) == true)
			{
				appliedAnyEffect = true;
			}
		}

		if (usable?.CastSkill != null)
		{
			if (caster == null || skillLoadout == null || skillCooldowns == null)
			{
				if (!appliedAnyEffect)
				{
					failureReason = ItemUseFailureReason.SkillCastBlocked;
					return false;
				}
			}
			else if (QueuedSkillCastService.TryQueueSkill(caster, usable.CastSkill, skillLoadout, skillCooldowns, aimController, chainTracker, out _))
			{
				appliedAnyEffect = true;
			}
			else if (!appliedAnyEffect)
			{
				failureReason = ItemUseFailureReason.SkillCastBlocked;
				return false;
			}
		}

		if (!appliedAnyEffect)
		{
			failureReason = ItemUseFailureReason.NoEffect;
			return false;
		}

		return true;
	}

	public static bool TryUseFromContainer(
		GridContainerComponent sourceContainer,
		string instanceId,
		EquipmentComponent? equipment,
		HealthComponent? health,
		ActorSkillResourceController? resources,
		Node3D? caster,
		ActorSkillLoadout? skillLoadout,
		ActorSkillCooldownController? skillCooldowns,
		CombatAimController? aimController,
		SkillChainTracker? chainTracker,
		out ItemInstance? usedItem,
		out ItemUseFailureReason failureReason)
	{
		usedItem = null;
		failureReason = ItemUseFailureReason.None;

		if (!sourceContainer.ContainsItem(instanceId))
		{
			failureReason = ItemUseFailureReason.MissingItem;
			return false;
		}

		ContainerItemRecord record = sourceContainer.GetRequiredRecord(instanceId);
		usedItem = record.Item;
		if (!TryApplyItemEffects(record.Item, record.Item.Definition, equipment, health, resources, caster, skillLoadout, skillCooldowns, aimController, chainTracker, out failureReason))
		{
			return false;
		}

		if (usedItem.HasLimitedUses)
		{
			sourceContainer.TryConsumeItemUse(instanceId);
		}
		else if (usedItem.Definition.WeaponSkill?.ConsumeOnInstall == true || usedItem.Definition.Usable?.ConsumeOnUse == true)
		{
			sourceContainer.TryConsumeItemStack(instanceId, 1);
		}

		return true;
	}
}
