using BattleHarvesterStudy.Items;
using Godot;

namespace BattleHarvesterStudy.Combat.Firearms;

public sealed class FirearmResolvedStats
{
	public required FirearmFireMode FireMode { get; init; }
	public required AmmoType AmmoType { get; init; }
	public required int BaseDamagePerPellet { get; init; }
	public required int PelletCount { get; init; }
	public required float FireRate { get; init; }
	public required int MagazineCapacity { get; init; }
	public required float EffectiveRange { get; init; }
	public required float SevereFalloffRange { get; init; }
	public required float BaseHitChance { get; init; }
	public required float Precision { get; init; }
	public required float RecoilControl { get; init; }
	public required float Handling { get; init; }
	public required float HipFireAccuracy { get; init; }
	public required float HipFireConeAngleDegrees { get; init; }
	public required float AimConeAngleDegrees { get; init; }
	public required bool CanAimWhileMoving { get; init; }
	public required float ReloadDuration { get; init; }
	public required float AimBaseBonus { get; init; }
	public required float MovingAimPenalty { get; init; }
	public required float TargetMovingPenalty { get; init; }
	public required float SevereRangeHitPenalty { get; init; }
	public required float FalloffRangeHitPenalty { get; init; }
	public required float FalloffRangeDamageMultiplier { get; init; }
	public required float SevereRangeDamageMultiplier { get; init; }
	public required float AimStaminaDrainPerSecond { get; init; }

	public float GetMaxAimBonus()
	{
		return 25.0f * Mathf.Clamp(Precision / 100.0f, 0.0f, 1.0f);
	}

	public float GetAimAccumulationDuration()
	{
		return Mathf.Lerp(2.0f, 1.0f, Mathf.Clamp(Handling / 100.0f, 0.0f, 1.0f));
	}

	public float GetAimStaminaDrainPerSecond()
	{
		float handlingAlpha = Mathf.Clamp(Handling / 100.0f, 0.0f, 1.0f);
		return AimStaminaDrainPerSecond * Mathf.Lerp(1.0f, 0.5f, handlingAlpha);
	}

	public float GetHipFireBonus()
	{
		return Mathf.Lerp(0.0f, 25.0f, Mathf.Clamp(HipFireAccuracy / 100.0f, 0.0f, 1.0f));
	}

	public float GetHipFireMovingPenalty()
	{
		return 10.0f;
	}

	public float GetRecoilFloorMultiplier()
	{
		return Mathf.Lerp(0.5f, 1.0f, Mathf.Clamp(RecoilControl / 100.0f, 0.0f, 1.0f));
	}

	public float GetRecoilStepMultiplier()
	{
		return Mathf.Lerp(0.12f, 0.0f, Mathf.Clamp(RecoilControl / 100.0f, 0.0f, 1.0f));
	}
}
