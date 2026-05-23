using Godot;
using BattleHarvesterStudy.Attributes;
using BattleHarvesterStudy.Equipment;
using BattleHarvesterStudy.Locomotion;

namespace BattleHarvesterStudy.Combat;

[GlobalClass]
public partial class DamageEffectDefinition : EffectDefinition
{
	[Export]
	public int Damage { get; set; } = 10;

	[Export]
	public DamageSchool DamageSchool { get; set; } = DamageSchool.Physical;

	[Export]
	public DamageDeliveryModel DeliveryModel { get; set; } = DamageDeliveryModel.Direct;

	[Export(PropertyHint.Range, "0,10,0.01")]
	public float AttackPowerScaling { get; set; } = 0.0f;

	[Export(PropertyHint.Range, "0,10,0.01")]
	public float MagicPowerScaling { get; set; } = 0.0f;

	[Export]
	public bool CanCrit { get; set; } = true;

	[Export]
	public float KnockbackForce { get; set; } = 2.0f;

	[Export]
	public bool CausesForcedMovement { get; set; } = true;

	[Export]
	public float ForcedMovementDuration { get; set; } = 0.10f;

	[Export]
	public int ForcedMovementPriority { get; set; } = 10;

	[Export(PropertyHint.Range, "0,10,1")]
	public int InterruptStrength { get; set; } = 1;

	[Export]
	public bool IgnoresInterruptArmor { get; set; }

	public override void Apply(HitResult hitResult)
	{
		StatsComponent? attackerStats = hitResult.Caster.GetNodeOrNull<StatsComponent>("Components/Stats");
		EquipmentComponent? attackerEquipment = hitResult.Caster.GetNodeOrNull<EquipmentComponent>("Components/Equipment");
		ItemInstance? activeWeaponItem = attackerEquipment?.GetActiveWeaponItem();
		WeaponEquipmentDefinition? activeWeapon = activeWeaponItem?.Definition.Equipment as WeaponEquipmentDefinition;
		float weaponConditionDamageMultiplier = activeWeaponItem?.HasDurability == true && activeWeaponItem.CurrentDurability <= 0.0f
			? activeWeapon?.BrokenDamageMultiplier ?? 1.0f
			: 1.0f;
		Node3D? targetActor = hitResult.Target.GetOwner<Node3D>();
		weaponConditionDamageMultiplier *= ResolveWeaponTacticalMultiplier(activeWeapon, targetActor);
		StatsComponent? defenderStats = targetActor?.GetNodeOrNull<StatsComponent>("Components/Stats");
		ArmorComponent? defenderArmor = targetActor?.GetNodeOrNull<ArmorComponent>("Components/Armor");
		DamageCalculationResult calculatedDamage = CombatStatFormulas.CalculateDamage(new DamageCalculationInput
		{
			BaseDamage = Damage,
			AttackPowerScaling = AttackPowerScaling,
			MagicPowerScaling = MagicPowerScaling,
			School = DamageSchool,
			DeliveryModel = DeliveryModel,
			CanCrit = CanCrit,
			SkillMultiplier = hitResult.Skill.SkillMultiplier,
			SkillPowerTier = hitResult.Skill.PowerTier,
			DestructionMultiplier = hitResult.Skill.DestructionMultiplier,
			BypassArmorPoint = hitResult.Skill.BypassArmorPoint,
			ActiveWeapon = activeWeapon,
			WeaponConditionDamageMultiplier = weaponConditionDamageMultiplier,
			AttackerStats = attackerStats,
			DefenderStats = defenderStats,
			DefenderArmor = defenderArmor,
		});
		ConsumeWeaponDurability(activeWeaponItem, activeWeapon, defenderStats, hitResult.Skill, calculatedDamage);
		Vector3 knockback = hitResult.HitDirection * KnockbackForce;
		hitResult.Target.TakeDamage(new DamageInfo(
			calculatedDamage.FinalDamage,
			knockback,
			hitResult.Caster,
			hitResult.Skill.SkillId,
			CausesForcedMovement,
			ForcedMovementDuration,
			ForcedMovementPriority,
			InterruptStrength,
			IgnoresInterruptArmor,
			calculatedDamage.BaseDamage,
			calculatedDamage.IsCritical,
			calculatedDamage.RawDamage,
			calculatedDamage.TierAdjustedDamage,
			calculatedDamage.ArmorAbsorbed,
			calculatedDamage.PostArmorDamage,
			calculatedDamage.TierDelta,
			DamageSchool,
			DeliveryModel,
			calculatedDamage.DurabilityDamage,
			calculatedDamage.PenetrationEffectiveness,
			calculatedDamage.ArmorLossResistance,
			calculatedDamage.EffectiveAbsorbRate
		));
	}

	private static float ResolveWeaponTacticalMultiplier(WeaponEquipmentDefinition? weapon, Node3D? targetActor)
	{
		if (weapon == null || targetActor == null)
		{
			return 1.0f;
		}

		float multiplier = 1.0f;
		SkillChainTracker? targetChainTracker = targetActor.GetNodeOrNull<SkillChainTracker>("Components/SkillChainTracker");
		if (weapon.RecoveryPunishMultiplier > 1.0f
			&& weapon.HasTrait(WeaponTrait.RecoveryPunish)
			&& targetChainTracker?.CurrentPhase == SkillPresentationPhase.Recovery)
		{
			multiplier *= weapon.RecoveryPunishMultiplier;
		}

		ForcedMovementComponent? targetForcedMovement = targetActor.GetNodeOrNull<ForcedMovementComponent>("Components/ForcedMovement");
		if (weapon.StunnedTargetMultiplier > 1.0f && targetForcedMovement?.HasActiveRequest == true)
		{
			multiplier *= weapon.StunnedTargetMultiplier;
		}

		return multiplier;
	}

	private static void ConsumeWeaponDurability(
		ItemInstance? weaponItem,
		WeaponEquipmentDefinition? weapon,
		StatsComponent? defenderStats,
		SkillDefinition skill,
		DamageCalculationResult calculatedDamage)
	{
		if (weaponItem == null || weapon == null || !weaponItem.HasDurability)
		{
			return;
		}

		if (weaponItem.CurrentDurability <= 0.0f)
		{
			weaponItem.ApplyDurabilityMaxDamage(calculatedDamage.RawDamage * weapon.StructuralWearRatio);
			return;
		}

		float armorTier = defenderStats?.GetValue(StatType.ArmorTier) ?? 0.0f;
		float tierPenalty = armorTier > weapon.GetBasePenetrationTier() + 2.0f ? 3.0f : 1.0f;
		weaponItem.ApplyDurabilityDamage(weapon.GetDurabilityLoss(skill.ActionType) * tierPenalty);
	}
}
