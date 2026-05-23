using System.Collections.Generic;
using BattleHarvesterStudy.Equipment;
using BattleHarvesterStudy.Items;

namespace BattleHarvesterStudy.Combat.Firearms;

public sealed class FirearmStatModifier
{
	public FirearmFireMode? FireModeOverride { get; init; }
	public AmmoType? AmmoTypeOverride { get; init; }
	public float BaseDamagePerPelletFlat { get; init; }
	public float BaseDamagePerPelletMultiplier { get; init; } = 1.0f;
	public float PelletCountFlat { get; init; }
	public float FireRateFlat { get; init; }
	public float FireRateMultiplier { get; init; } = 1.0f;
	public float MagazineCapacityFlat { get; init; }
	public float MagazineCapacityMultiplier { get; init; } = 1.0f;
	public float EffectiveRangeFlat { get; init; }
	public float EffectiveRangeMultiplier { get; init; } = 1.0f;
	public float SevereFalloffRangeFlat { get; init; }
	public float SevereFalloffRangeMultiplier { get; init; } = 1.0f;
	public float BaseHitChanceFlat { get; init; }
	public float PrecisionFlat { get; init; }
	public float RecoilControlFlat { get; init; }
	public float HandlingFlat { get; init; }
	public float HipFireAccuracyFlat { get; init; }
	public float ReloadDurationFlat { get; init; }
	public float ReloadDurationMultiplier { get; init; } = 1.0f;
}

public interface IFirearmStatModifierSource
{
	IEnumerable<FirearmStatModifier> GetFirearmStatModifiers(FirearmWeaponDefinition firearm, ItemInstance firearmItem);
}
