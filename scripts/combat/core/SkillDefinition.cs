using Godot;
using Godot.Collections;
using BattleHarvesterStudy.Equipment;

namespace BattleHarvesterStudy.Combat;

[GlobalClass]
public partial class SkillDefinition : Resource
{
	[Export]
	public string SkillId { get; set; } = "basic_attack";

	[Export]
	public string DisplayName { get; set; } = "Basic Attack";

	[Export]
	public Array<string> SkillTags { get; set; } = new();

	[Export]
	public SkillLoadoutCategory LoadoutCategory { get; set; } = SkillLoadoutCategory.Weapon;

	[Export]
	public CombatActionType ActionType { get; set; } = CombatActionType.Light;

	[Export(PropertyHint.Range, "0,10,0.01")]
	public float SkillMultiplier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0,20,0.01")]
	public float PowerTier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0,10,0.01")]
	public float DestructionMultiplier { get; set; } = 1.0f;

	[Export]
	public bool BypassArmorPoint { get; set; }

	[Export(PropertyHint.Flags, "Sharp,Blunt,Projectile,Flexible,Mechanism")]
	public int AllowedWeaponArchetypeMask { get; set; }

	[Export(PropertyHint.Flags, "Sword,Katana,Greatsword,Dagger,Spear,Hammer,Axe,Mace,ThrowingKnife,BoltLauncher,Whip,ChainBlade,Drill,Flamethrower")]
	public int AllowedWeaponFamilyMask { get; set; }

	[Export(PropertyHint.Flags, "RecoveryPunish,HeavySwing,Bleed,Execution,MultiHit,Sustained,ArmorBreak,Reach,Bypass")]
	public int RequiredWeaponTraitMask { get; set; }

	[Export]
	public string StartupLabel { get; set; } = "START";

	[Export]
	public string ActiveLabel { get; set; } = "ACTIVE";

	[Export]
	public string RecoveryLabel { get; set; } = "RECOVERY";

	[Export(PropertyHint.Range, "0,5,0.01")]
	public float StartupSeconds { get; set; } = 0.15f;

	[Export(PropertyHint.Range, "0,5,0.01")]
	public float ActiveSeconds { get; set; } = 0.12f;

	[Export(PropertyHint.Range, "0,5,0.01")]
	public float RecoverySeconds { get; set; } = 0.20f;

	[Export(PropertyHint.Range, "0,60,0.01")]
	public float BaseCooldownSeconds { get; set; } = 0.0f;

	[Export(PropertyHint.Range, "0,10,0.01")]
	public float BaseGlobalCooldownSeconds { get; set; } = 0.0f;

	[Export]
	public string CooldownGroupId { get; set; } = "";

	[Export]
	public bool IgnoreGlobalCooldown { get; set; }

	[Export(PropertyHint.Range, "0,999,0.1")]
	public float ResourceCost { get; set; } = 0.0f;

	[Export]
	public Array<SkillCastRequirement> CastRequirements { get; set; } = new();

	[Export]
	public bool CanBufferNextSkillDuringStartup { get; set; }

	[Export]
	public bool CanBufferNextSkillDuringActive { get; set; }

	[Export]
	public bool CanBufferNextSkillDuringRecovery { get; set; } = true;

	[Export]
	public bool RequireHitConfirmForBuffer { get; set; }

	[Export]
	public bool CanDashCancelDuringStartup { get; set; }

	[Export]
	public bool CanDashCancelDuringActive { get; set; }

	[Export]
	public bool CanDashCancelDuringRecovery { get; set; } = true;

	[Export]
	public bool CanBeInterruptedDuringStartup { get; set; } = true;

	[Export]
	public bool CanBeInterruptedDuringActive { get; set; } = true;

	[Export]
	public bool CanBeInterruptedDuringRecovery { get; set; } = true;

	[Export(PropertyHint.Range, "0,10,1")]
	public int InterruptArmorDuringStartup { get; set; }

	[Export(PropertyHint.Range, "0,10,1")]
	public int InterruptArmorDuringActive { get; set; }

	[Export(PropertyHint.Range, "0,10,1")]
	public int InterruptArmorDuringRecovery { get; set; }

	[Export]
	public AttackBehaviorDefinition? AttackBehavior { get; set; }

	[Export]
	public SkillPresentationDefinition? Presentation { get; set; }

	[Export]
	public Array<EffectDefinition> Effects { get; set; } = new();

	public bool HasSkillTag(string tag)
	{
		return ContainsTag(SkillTags, tag);
	}

	public bool CanBufferNextSkill(SkillPresentationPhase phase, bool hitConfirmed)
	{
		bool phaseAllowsBuffer = phase switch
		{
			SkillPresentationPhase.Startup => CanBufferNextSkillDuringStartup,
			SkillPresentationPhase.Active => CanBufferNextSkillDuringActive,
			SkillPresentationPhase.Recovery => CanBufferNextSkillDuringRecovery,
			_ => false
		};

		if (!phaseAllowsBuffer)
		{
			return false;
		}

		return !RequireHitConfirmForBuffer || hitConfirmed;
	}

	public bool CanDashCancel(SkillPresentationPhase phase)
	{
		return phase switch
		{
			SkillPresentationPhase.Startup => CanDashCancelDuringStartup,
			SkillPresentationPhase.Active => CanDashCancelDuringActive,
			SkillPresentationPhase.Recovery => CanDashCancelDuringRecovery,
			_ => false
		};
	}

	public bool CanBeInterrupted(SkillPresentationPhase phase)
	{
		return phase switch
		{
			SkillPresentationPhase.Startup => CanBeInterruptedDuringStartup,
			SkillPresentationPhase.Active => CanBeInterruptedDuringActive,
			SkillPresentationPhase.Recovery => CanBeInterruptedDuringRecovery,
			_ => false
		};
	}

	public bool SupportsWeapon(WeaponEquipmentDefinition? weapon)
	{
		if (AllowedWeaponArchetypeMask == 0
			&& AllowedWeaponFamilyMask == 0
			&& RequiredWeaponTraitMask == 0)
		{
			return true;
		}

		if (weapon == null)
		{
			return false;
		}

		return WeaponClassificationUtility.MatchesAny(weapon.WeaponArchetypeId, AllowedWeaponArchetypeMask)
			&& WeaponClassificationUtility.MatchesAny(weapon.WeaponFamilyId, AllowedWeaponFamilyMask)
			&& WeaponClassificationUtility.ContainsAll(weapon.WeaponTraitMask, RequiredWeaponTraitMask);
	}

	public string GetWeaponCompatibilitySummary(bool chinese)
	{
		if (AllowedWeaponArchetypeMask == 0 && AllowedWeaponFamilyMask == 0 && RequiredWeaponTraitMask == 0)
		{
			return chinese ? "通用" : "Universal";
		}

		Array<string> parts = [];
		if (AllowedWeaponArchetypeMask != 0)
		{
			parts.Add(chinese
				? $"大类 {WeaponClassificationUtility.FormatArchetypes(AllowedWeaponArchetypeMask, true)}"
				: $"Archetype {WeaponClassificationUtility.FormatArchetypes(AllowedWeaponArchetypeMask, false)}");
		}

		if (AllowedWeaponFamilyMask != 0)
		{
			parts.Add(chinese
				? $"家族 {WeaponClassificationUtility.FormatFamilies(AllowedWeaponFamilyMask, true)}"
				: $"Family {WeaponClassificationUtility.FormatFamilies(AllowedWeaponFamilyMask, false)}");
		}

		if (RequiredWeaponTraitMask != 0)
		{
			parts.Add(chinese
				? $"特性 {WeaponClassificationUtility.FormatTraits(RequiredWeaponTraitMask, true)}"
				: $"Traits {WeaponClassificationUtility.FormatTraits(RequiredWeaponTraitMask, false)}");
		}

		return string.Join(" / ", parts);
	}

	public int GetInterruptArmor(SkillPresentationPhase phase)
	{
		return phase switch
		{
			SkillPresentationPhase.Startup => InterruptArmorDuringStartup,
			SkillPresentationPhase.Active => InterruptArmorDuringActive,
			SkillPresentationPhase.Recovery => InterruptArmorDuringRecovery,
			_ => 0
		};
	}

	private static bool ContainsTag(Array<string> tags, string tag)
	{
		if (string.IsNullOrWhiteSpace(tag))
		{
			return false;
		}

		foreach (string existingTag in tags)
		{
			if (string.Equals(existingTag, tag, System.StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}

		return false;
	}
}
