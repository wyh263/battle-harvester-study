using Godot;
using BattleHarvesterStudy.Attributes;
using BattleHarvesterStudy.Combat;
using BattleHarvesterStudy.Equipment;

namespace BattleHarvesterStudy.Items;

public static class EquippedItemUseService
{
	public static bool TryUseEquippedItem(
		EquipmentComponent equipment,
		EquipmentSlotType slotType,
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
		usedItem = equipment.GetEquippedItem(slotType);
		if (usedItem == null)
		{
			failureReason = ItemUseFailureReason.MissingItem;
			return false;
		}

		if (!ItemUseService.TryApplyItemEffects(usedItem, usedItem.Definition, equipment, health, resources, caster, skillLoadout, skillCooldowns, aimController, chainTracker, out failureReason))
		{
			return false;
		}

		if (usedItem.HasLimitedUses)
		{
			equipment.TryConsumeEquippedItemUse(slotType);
		}
		else if (usedItem.Definition.WeaponSkill?.ConsumeOnInstall == true || usedItem.Definition.Usable?.ConsumeOnUse == true)
		{
			equipment.TryConsumeEquippedItem(slotType, 1);
		}

		return true;
	}
}
