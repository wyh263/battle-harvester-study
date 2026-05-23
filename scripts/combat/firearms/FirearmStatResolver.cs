using System.Collections.Generic;
using BattleHarvesterStudy.Equipment;
using BattleHarvesterStudy.Items;
using Godot;

namespace BattleHarvesterStudy.Combat.Firearms;

public static class FirearmStatResolver
{
	public static FirearmResolvedStats Resolve(FirearmWeaponDefinition firearm)
	{
		return Resolve(firearm, null, []);
	}

	public static FirearmResolvedStats Resolve(FirearmWeaponDefinition firearm, ItemInstance? firearmItem)
	{
		return Resolve(firearm, firearmItem, []);
	}

	public static FirearmResolvedStats Resolve(
		FirearmWeaponDefinition firearm,
		ItemInstance? firearmItem,
		IEnumerable<IFirearmStatModifierSource> modifierSources)
	{
		FirearmMutableStats mutable = FirearmMutableStats.FromDefinition(firearm);
		if (firearmItem != null)
		{
			foreach (IFirearmStatModifierSource source in modifierSources)
			{
				foreach (FirearmStatModifier modifier in source.GetFirearmStatModifiers(firearm, firearmItem))
				{
					mutable.Apply(modifier);
				}
			}
		}

		return mutable.ToResolved();
	}

	private sealed class FirearmMutableStats
	{
		public FirearmFireMode FireMode { get; private set; }
		public AmmoType AmmoType { get; private set; }
		public float BaseDamagePerPellet { get; private set; }
		public float PelletCount { get; private set; }
		public float FireRate { get; private set; }
		public float MagazineCapacity { get; private set; }
		public float EffectiveRange { get; private set; }
		public float SevereFalloffRange { get; private set; }
		public float BaseHitChance { get; private set; }
		public float Precision { get; private set; }
		public float RecoilControl { get; private set; }
		public float Handling { get; private set; }
		public float HipFireAccuracy { get; private set; }
		public float HipFireConeAngleDegrees { get; private set; }
		public float AimConeAngleDegrees { get; private set; }
		public bool CanAimWhileMoving { get; private set; }
		public float ReloadDuration { get; private set; }
		public float AimBaseBonus { get; private set; }
		public float MovingAimPenalty { get; private set; }
		public float TargetMovingPenalty { get; private set; }
		public float SevereRangeHitPenalty { get; private set; }
		public float FalloffRangeHitPenalty { get; private set; }
		public float FalloffRangeDamageMultiplier { get; private set; }
		public float SevereRangeDamageMultiplier { get; private set; }
		public float AimStaminaDrainPerSecond { get; private set; }

		public static FirearmMutableStats FromDefinition(FirearmWeaponDefinition firearm)
		{
			return new FirearmMutableStats
			{
				FireMode = firearm.FireMode,
				AmmoType = firearm.AmmoType,
				BaseDamagePerPellet = firearm.BaseDamagePerPellet,
				PelletCount = firearm.PelletCount,
				FireRate = firearm.FireRate,
				MagazineCapacity = firearm.MagazineCapacity,
				EffectiveRange = firearm.EffectiveRange,
				SevereFalloffRange = firearm.SevereFalloffRange,
				BaseHitChance = firearm.BaseHitChance,
				Precision = firearm.Precision,
				RecoilControl = firearm.RecoilControl,
				Handling = firearm.Handling,
				HipFireAccuracy = firearm.HipFireAccuracy,
				HipFireConeAngleDegrees = firearm.HipFireConeAngleDegrees,
				AimConeAngleDegrees = firearm.AimConeAngleDegrees,
				CanAimWhileMoving = firearm.CanAimWhileMoving,
				ReloadDuration = firearm.ReloadDuration,
				AimBaseBonus = firearm.AimBaseBonus,
				MovingAimPenalty = firearm.MovingAimPenalty,
				TargetMovingPenalty = firearm.TargetMovingPenalty,
				SevereRangeHitPenalty = firearm.SevereRangeHitPenalty,
				FalloffRangeHitPenalty = firearm.FalloffRangeHitPenalty,
				FalloffRangeDamageMultiplier = firearm.FalloffRangeDamageMultiplier,
				SevereRangeDamageMultiplier = firearm.SevereRangeDamageMultiplier,
				AimStaminaDrainPerSecond = firearm.AimStaminaDrainPerSecond
			};
		}

		public void Apply(FirearmStatModifier modifier)
		{
			FireMode = modifier.FireModeOverride ?? FireMode;
			AmmoType = modifier.AmmoTypeOverride ?? AmmoType;
			BaseDamagePerPellet = (BaseDamagePerPellet + modifier.BaseDamagePerPelletFlat) * modifier.BaseDamagePerPelletMultiplier;
			PelletCount += modifier.PelletCountFlat;
			FireRate = (FireRate + modifier.FireRateFlat) * modifier.FireRateMultiplier;
			MagazineCapacity = (MagazineCapacity + modifier.MagazineCapacityFlat) * modifier.MagazineCapacityMultiplier;
			EffectiveRange = (EffectiveRange + modifier.EffectiveRangeFlat) * modifier.EffectiveRangeMultiplier;
			SevereFalloffRange = (SevereFalloffRange + modifier.SevereFalloffRangeFlat) * modifier.SevereFalloffRangeMultiplier;
			BaseHitChance += modifier.BaseHitChanceFlat;
			Precision += modifier.PrecisionFlat;
			RecoilControl += modifier.RecoilControlFlat;
			Handling += modifier.HandlingFlat;
			HipFireAccuracy += modifier.HipFireAccuracyFlat;
			ReloadDuration = (ReloadDuration + modifier.ReloadDurationFlat) * modifier.ReloadDurationMultiplier;
		}

		public FirearmResolvedStats ToResolved()
		{
			float effectiveRange = Mathf.Max(0.5f, EffectiveRange);
			return new FirearmResolvedStats
			{
				FireMode = FireMode,
				AmmoType = AmmoType,
				BaseDamagePerPellet = Mathf.Max(1, Mathf.RoundToInt(BaseDamagePerPellet)),
				PelletCount = Mathf.Max(1, Mathf.RoundToInt(PelletCount)),
				FireRate = Mathf.Max(0.01f, FireRate),
				MagazineCapacity = Mathf.Max(0, Mathf.RoundToInt(MagazineCapacity)),
				EffectiveRange = effectiveRange,
				SevereFalloffRange = Mathf.Max(effectiveRange, SevereFalloffRange),
				BaseHitChance = Mathf.Clamp(BaseHitChance, 0.0f, 100.0f),
				Precision = Mathf.Clamp(Precision, 0.0f, 100.0f),
				RecoilControl = Mathf.Clamp(RecoilControl, 0.0f, 100.0f),
				Handling = Mathf.Clamp(Handling, 0.0f, 100.0f),
				HipFireAccuracy = Mathf.Clamp(HipFireAccuracy, 0.0f, 100.0f),
				HipFireConeAngleDegrees = Mathf.Clamp(HipFireConeAngleDegrees, 5.0f, 180.0f),
				AimConeAngleDegrees = Mathf.Clamp(AimConeAngleDegrees, 1.0f, 120.0f),
				CanAimWhileMoving = CanAimWhileMoving,
				ReloadDuration = Mathf.Max(0.1f, ReloadDuration),
				AimBaseBonus = Mathf.Max(0.0f, AimBaseBonus),
				MovingAimPenalty = Mathf.Max(0.0f, MovingAimPenalty),
				TargetMovingPenalty = Mathf.Max(0.0f, TargetMovingPenalty),
				SevereRangeHitPenalty = Mathf.Max(0.0f, SevereRangeHitPenalty),
				FalloffRangeHitPenalty = Mathf.Max(0.0f, FalloffRangeHitPenalty),
				FalloffRangeDamageMultiplier = Mathf.Clamp(FalloffRangeDamageMultiplier, 0.0f, 1.0f),
				SevereRangeDamageMultiplier = Mathf.Clamp(SevereRangeDamageMultiplier, 0.0f, 1.0f),
				AimStaminaDrainPerSecond = Mathf.Max(0.0f, AimStaminaDrainPerSecond)
			};
		}
	}
}
