using Godot;
using Godot.Collections;

namespace BattleHarvesterStudy.Equipment;

[GlobalClass]
public partial class WeaponEquipmentDefinition : EquipmentDefinition
{
	[Export]
	public WeaponArchetype WeaponArchetypeId { get; set; } = WeaponArchetype.Sharp;

	[Export]
	public WeaponFamily WeaponFamilyId { get; set; } = WeaponFamily.Sword;

	[Export(PropertyHint.Flags, "RecoveryPunish,HeavySwing,Bleed,Execution,MultiHit,Sustained,ArmorBreak,Reach,Bypass")]
	public int WeaponTraitMask { get; set; }

	[Export]
	public WeaponMoveSetDefinition? MoveSet { get; set; }

	[Export(PropertyHint.Range, "0,4,1")]
	public int WeaponSkillSlotCount { get; set; } = 2;

	[Export]
	public Array<string> AllowedWeaponSkillTags { get; set; } = [];

	[Export]
	public DamageSchool DamageSchool { get; set; } = DamageSchool.Physical;

	[Export]
	public DamageDeliveryModel DeliveryModel { get; set; } = DamageDeliveryModel.WeaponDriven;

	[Export(PropertyHint.Range, "0.1,5,0.01")]
	public float AttackSpeedMultiplier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0.1,5,0.01")]
	public float BaseMultiplier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0,20,0.01")]
	public float PenetrationTier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "-5,5,0.01")]
	public float PenetrationTierBias { get; set; } = 0.0f;

	[Export(PropertyHint.Range, "0,10,0.01")]
	public float DestructionMultiplierBias { get; set; } = 1.0f;

	[Export]
	public bool GrantsBypassArmorPoint { get; set; }

	[Export(PropertyHint.Range, "1,9999,1")]
	public float MaxDurability { get; set; } = 100.0f;

	[Export(PropertyHint.Range, "0,1,0.01")]
	public float CritRate { get; set; } = 0.05f;

	[Export(PropertyHint.Range, "1,10,0.01")]
	public float CritDamage { get; set; } = 1.5f;

	[Export(PropertyHint.Range, "0,100,0.01")]
	public float LightDurabilityLoss { get; set; } = 0.2f;

	[Export(PropertyHint.Range, "0,100,0.01")]
	public float HeavyDurabilityLoss { get; set; } = 0.5f;

	[Export(PropertyHint.Range, "0,100,0.01")]
	public float SkillDurabilityLoss { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0,100,0.01")]
	public float UltimateDurabilityLoss { get; set; } = 5.0f;

	[Export(PropertyHint.Range, "0,1,0.01")]
	public float BrokenDamageMultiplier { get; set; } = 0.35f;

	[Export(PropertyHint.Range, "0,1,0.01")]
	public float StructuralWearRatio { get; set; } = 0.05f;

	[Export(PropertyHint.Range, "1,5,0.01")]
	public float RecoveryPunishMultiplier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "1,5,0.01")]
	public float StunnedTargetMultiplier { get; set; } = 1.0f;

	[Export(PropertyHint.Range, "0,0.95,0.01")]
	public float BufferDamageReduction { get; set; } = 0.0f;

	[Export(PropertyHint.Range, "0,1,0.01")]
	public float ExecutionHealthThreshold { get; set; } = 0.0f;

	[Export(PropertyHint.Range, "0,99,1")]
	public int ExecutionMinBleedStacks { get; set; } = 0;

	[Export(PropertyHint.Range, "0,20,0.01")]
	public float BleedCycleSeconds { get; set; } = 0.0f;

	[Export(PropertyHint.Range, "0,1,0.01")]
	public float FirstHitCritRateBonus { get; set; } = 0.0f;

	[Export(PropertyHint.Range, "0,5,0.01")]
	public float FirstHitCritDamageBonus { get; set; } = 0.0f;

	public float GetBasePenetrationTier()
	{
		return Mathf.Max(PenetrationTier, Tier) + PenetrationTierBias;
	}

	public int GetWeaponSkillSlotCount()
	{
		return Mathf.Max(0, WeaponSkillSlotCount);
	}

	public bool HasTrait(WeaponTrait trait)
	{
		return (WeaponTraitMask & (int)trait) != 0;
	}

	public bool SupportsWeaponSkill(SkillDefinition? skill)
	{
		if (skill == null || skill.LoadoutCategory != SkillLoadoutCategory.Weapon || GetWeaponSkillSlotCount() <= 0)
		{
			return false;
		}

		if (AllowedWeaponSkillTags.Count == 0)
		{
			return true;
		}

		foreach (string allowedTag in AllowedWeaponSkillTags)
		{
			if (skill.HasSkillTag(allowedTag))
			{
				return true;
			}
		}

		return false;
	}

	public float GetDurabilityLoss(CombatActionType actionType)
	{
		return actionType switch
		{
			CombatActionType.Heavy => HeavyDurabilityLoss,
			CombatActionType.Skill => SkillDurabilityLoss,
			CombatActionType.Ultimate => UltimateDurabilityLoss,
			_ => LightDurabilityLoss,
		};
	}

	public WeaponEquipmentDefinition()
	{
		Kind = EquipmentKind.Weapon;
	}
}
