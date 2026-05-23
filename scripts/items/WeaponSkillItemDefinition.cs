using Godot;
using BattleHarvesterStudy.Equipment;

namespace BattleHarvesterStudy.Items;

[GlobalClass]
public partial class WeaponSkillItemDefinition : Resource
{
	[Export]
	public SkillDefinition? Skill { get; set; }

	[Export(PropertyHint.Range, "1,999,1")]
	public int GrantedUses { get; set; } = 10;

	[Export]
	public bool ConsumeOnInstall { get; set; } = true;

	[Export]
	public bool ConsumeUsesOnCast { get; set; } = true;

	[Export]
	public bool CanBeLearned { get; set; } = true;

	[Export]
	public WeaponSkillUnlockConditionType UnlockConditionType { get; set; } = WeaponSkillUnlockConditionType.UseCount;

	[Export(PropertyHint.Range, "0,9999,1")]
	public int RequiredUseCount { get; set; } = 25;

	[Export(PropertyHint.Range, "0,9999,1")]
	public int RequiredKillCount { get; set; }

	[Export(PropertyHint.Range, "0,9999,1")]
	public int RequiredBossKillCount { get; set; }

	[Export(PropertyHint.Flags, "Sharp,Blunt,Projectile,Flexible,Mechanism")]
	public int AllowedWeaponArchetypeMask { get; set; }

	[Export(PropertyHint.Flags, "Sword,Katana,Greatsword,Dagger,Spear,Hammer,Axe,Mace,ThrowingKnife,BoltLauncher,Whip,ChainBlade,Drill,Flamethrower")]
	public int AllowedWeaponFamilyMask { get; set; }

	[Export(PropertyHint.Flags, "RecoveryPunish,HeavySwing,Bleed,Execution,MultiHit,Sustained,ArmorBreak,Reach,Bypass")]
	public int RequiredWeaponTraitMask { get; set; }

	public bool CanInstallOnWeapon(WeaponEquipmentDefinition? weapon)
	{
		if (weapon == null || Skill == null || Skill.LoadoutCategory != SkillLoadoutCategory.Weapon)
		{
			return false;
		}

		if (!weapon.SupportsWeaponSkill(Skill))
		{
			return false;
		}

		if (AllowedWeaponArchetypeMask != 0
			&& !WeaponClassificationUtility.MatchesAny(weapon.WeaponArchetypeId, AllowedWeaponArchetypeMask))
		{
			return false;
		}

		if (AllowedWeaponFamilyMask != 0
			&& !WeaponClassificationUtility.MatchesAny(weapon.WeaponFamilyId, AllowedWeaponFamilyMask))
		{
			return false;
		}

		if (RequiredWeaponTraitMask != 0
			&& !WeaponClassificationUtility.ContainsAll(weapon.WeaponTraitMask, RequiredWeaponTraitMask))
		{
			return false;
		}

		return true;
	}

	public InstalledWeaponSkillState CreateInstalledState(int slotIndex)
	{
		return CreateInstalledState(slotIndex, null, ItemAcquisitionState.Base, GrantedUses);
	}

	public InstalledWeaponSkillState CreateInstalledState(int slotIndex, ItemDefinition? sourceItemDefinition, ItemAcquisitionState sourceAcquisitionState, int grantedUsesOverride)
	{
		return InstalledWeaponSkillState.CreateInstalled(
			slotIndex,
			Skill ?? throw new System.InvalidOperationException("Weapon skill item is missing its skill definition."),
			grantedUsesOverride,
			sourceItemDefinition,
			sourceAcquisitionState,
			ConsumeUsesOnCast,
			CanBeLearned,
			UnlockConditionType,
			RequiredUseCount,
			RequiredKillCount,
			RequiredBossKillCount);
	}
}
