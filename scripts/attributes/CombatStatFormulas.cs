using Godot;
using BattleHarvesterStudy.Equipment;

namespace BattleHarvesterStudy.Attributes;

public readonly struct DamageCalculationResult
{
	public required int RawDamage { get; init; }
	public required int TierAdjustedDamage { get; init; }
	public required int ArmorAbsorbed { get; init; }
	public required int ArmorPointDamage { get; init; }
	public required int DurabilityDamage { get; init; }
	public required int PostArmorDamage { get; init; }
	public required int FinalDamage { get; init; }
	public required int BaseDamage { get; init; }
	public required bool IsCritical { get; init; }
	public required int TierDelta { get; init; }
	public required float PenetrationEffectiveness { get; init; }
	public required float ArmorLossResistance { get; init; }
	public required float EffectiveAbsorbRate { get; init; }
}

public static class CombatStatFormulas
{
	private static readonly float[] TierWeights = [0.15f, 0.20f, 0.30f, 0.45f, 0.65f];

	public static DamageCalculationResult CalculateDamage(DamageCalculationInput input)
	{
		float attackPower = input.AttackerStats?.GetValue(StatType.AttackPower) ?? 0.0f;
		float magicPower = input.AttackerStats?.GetValue(StatType.MagicPower) ?? 0.0f;
		float defense = Mathf.Max(0.0f, input.DefenderStats?.GetValue(StatType.Defense) ?? 0.0f);
		float magicResistance = Mathf.Max(0.0f, input.DefenderStats?.GetValue(StatType.MagicResistance) ?? 0.0f);
		float critChance = Mathf.Clamp(input.ActiveWeapon?.CritRate ?? input.AttackerStats?.GetValue(StatType.CritChance) ?? 0.0f, 0.0f, 1.0f);
		float critDamage = Mathf.Max(1.0f, input.ActiveWeapon?.CritDamage ?? input.AttackerStats?.GetValue(StatType.CritDamage) ?? 1.5f);
		float defenseK = Mathf.Max(1.0f, input.DefenderStats?.GetValue(StatType.DefenseReductionK) ?? 400.0f);
		float proficiency = Mathf.Clamp(input.AttackerStats?.GetValue(StatType.CombatProficiency) ?? 0.0f, 0.0f, 100.0f);
		float penetrationBonus = Mathf.Clamp(proficiency * 0.01f, 0.0f, 1.0f);

		float weaponMultiplier = Mathf.Max(0.0f, input.ActiveWeapon?.BaseMultiplier ?? 1.0f);
		float weaponDestructionMultiplier = Mathf.Max(0.0f, input.ActiveWeapon?.DestructionMultiplierBias ?? 1.0f);
		bool bypassArmorPoint = input.BypassArmorPoint || input.ActiveWeapon?.GrantsBypassArmorPoint == true || input.ActiveWeapon?.HasTrait(WeaponTrait.Bypass) == true;
		float skillMultiplier = Mathf.Max(0.0f, input.SkillMultiplier);
		float conditionMultiplier = Mathf.Max(0.0f, input.WeaponConditionDamageMultiplier);
		float rawDamage = input.DeliveryModel == DamageDeliveryModel.WeaponDriven
			? attackPower * weaponMultiplier * skillMultiplier * conditionMultiplier
			: input.BaseDamage + attackPower * input.AttackPowerScaling + magicPower * input.MagicPowerScaling;
		rawDamage = Mathf.Max(0.0f, rawDamage);
		int tierDelta = 0;
		float tierAdjustedDamage = rawDamage;
		float armorPointDamage = 0.0f;
		float durabilityDamage = 0.0f;
		float postArmorDamage = rawDamage;
		float penetrationEffectiveness = 1.0f;
		float armorLossResistance = 1.0f;
		float effectiveAbsorbRate = 0.0f;

		if (input.School == DamageSchool.Physical
			&& (input.DeliveryModel == DamageDeliveryModel.WeaponDriven || input.SkillPowerTier > 0.0f))
		{
			float weaponPenetrationTier = input.ActiveWeapon == null
				? 0.0f
				: input.ActiveWeapon.GetBasePenetrationTier();
			float statPenetrationTier = input.AttackerStats?.GetValue(StatType.WeaponPenetrationTier) ?? 0.0f;
			float basePenetrationTier = input.DeliveryModel == DamageDeliveryModel.WeaponDriven
				? Mathf.Max(Mathf.Max(weaponPenetrationTier, statPenetrationTier), input.SkillPowerTier)
				: input.SkillPowerTier;
			float penetrationTier = basePenetrationTier + penetrationBonus;
			float armorTier = input.DefenderStats?.GetValue(StatType.ArmorTier) ?? 0.0f;
			float baseAbsorbRate = Mathf.Clamp(input.DefenderStats?.GetValue(StatType.ArmorAbsorbRate) ?? 0.0f, 0.0f, 0.95f);
			float tierDeltaValue = penetrationTier - armorTier;
			tierDelta = Mathf.RoundToInt(tierDeltaValue);
			penetrationEffectiveness = ResolvePenetrationEffectiveness(tierDeltaValue, armorTier);
			armorLossResistance = armorTier > penetrationTier ? Mathf.Pow(0.5f, armorTier - penetrationTier) : 1.0f;

			if (bypassArmorPoint)
			{
				tierAdjustedDamage = rawDamage * penetrationEffectiveness;
				postArmorDamage = tierAdjustedDamage;
			}
			else if ((input.DefenderArmor?.CurrentArmorPoint ?? 0.0f) > 0.0f)
			{
				effectiveAbsorbRate = Mathf.Clamp(baseAbsorbRate / penetrationEffectiveness, 0.1f, 0.95f);
				postArmorDamage = rawDamage * (1.0f - effectiveAbsorbRate);
				armorPointDamage = rawDamage * effectiveAbsorbRate * armorLossResistance * Mathf.Max(0.0f, input.DestructionMultiplier) * weaponDestructionMultiplier;
			}
			else
			{
				tierAdjustedDamage = rawDamage * penetrationEffectiveness;
				postArmorDamage = tierAdjustedDamage;
				durabilityDamage = rawDamage * 0.05f;
			}
		}

		float mitigatedDamage = input.School switch
		{
			DamageSchool.True => postArmorDamage,
			DamageSchool.Magic or DamageSchool.Fire or DamageSchool.Poison => postArmorDamage * (1.0f - ResolveDamageReduction(magicResistance, defenseK)),
			_ => postArmorDamage * (1.0f - ResolveDamageReduction(defense, defenseK)),
		};

		bool isCritical = input.CanCrit && critChance > 0.0f && GD.Randf() < critChance;
		if (isCritical)
		{
			mitigatedDamage *= critDamage;
		}

		return new DamageCalculationResult
		{
			RawDamage = Mathf.Max(0, Mathf.RoundToInt(rawDamage)),
			TierAdjustedDamage = Mathf.Max(0, Mathf.RoundToInt(tierAdjustedDamage)),
			ArmorAbsorbed = Mathf.Max(0, Mathf.RoundToInt(armorPointDamage)),
			ArmorPointDamage = Mathf.Max(0, Mathf.RoundToInt(armorPointDamage)),
			DurabilityDamage = Mathf.Max(0, Mathf.RoundToInt(durabilityDamage)),
			PostArmorDamage = Mathf.Max(0, Mathf.RoundToInt(postArmorDamage)),
			BaseDamage = Mathf.Max(0, Mathf.RoundToInt(rawDamage)),
			FinalDamage = Mathf.Max(1, Mathf.RoundToInt(mitigatedDamage)),
			IsCritical = isCritical,
			TierDelta = tierDelta,
			PenetrationEffectiveness = penetrationEffectiveness,
			ArmorLossResistance = armorLossResistance,
			EffectiveAbsorbRate = effectiveAbsorbRate,
		};
	}

	private static float ResolveDamageReduction(float defense, float defenseK)
	{
		return defense <= 0.0f ? 0.0f : Mathf.Clamp(defense / (defense + defenseK), 0.0f, 0.95f);
	}

	private static float ResolvePenetrationEffectiveness(float tierDelta, float armorTier)
	{
		if (tierDelta >= 0.0f)
		{
			return 1.0f + tierDelta * 0.15f;
		}

		int weightIndex = Mathf.Clamp(Mathf.CeilToInt(armorTier) - 1, 0, TierWeights.Length - 1);
		return Mathf.Max(0.15f, 1.0f + tierDelta * TierWeights[weightIndex]);
	}
}
