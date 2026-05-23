using Godot;

namespace BattleHarvesterStudy.Equipment;

[GlobalClass]
public partial class FirearmWeaponDefinition : WeaponEquipmentDefinition
{
	public FirearmWeaponDefinition()
	{
		MaxDurability = 0.0f;
		BrokenDamageMultiplier = 1.0f;
		LightDurabilityLoss = 0.0f;
		HeavyDurabilityLoss = 0.0f;
		SkillDurabilityLoss = 0.0f;
		UltimateDurabilityLoss = 0.0f;
		StructuralWearRatio = 0.0f;
	}

	[Export] public FirearmFireMode FireMode { get; set; } = FirearmFireMode.SingleShot;
	[Export] public AmmoType AmmoType { get; set; } = AmmoType.RifleRound;
	[Export(PropertyHint.Range, "1,9999,1")] public int BaseDamagePerPellet { get; set; } = 10;
	[Export(PropertyHint.Range, "1,32,1")] public int PelletCount { get; set; } = 1;
	[Export(PropertyHint.Range, "0.1,20,0.01")] public float FireRate { get; set; } = 2.0f;
	[Export(PropertyHint.Range, "1,120,1")] public int MagazineCapacity { get; set; } = 6;
	[Export(PropertyHint.Range, "0.5,200,0.1")] public float EffectiveRange { get; set; } = 18.0f;
	[Export(PropertyHint.Range, "0.5,300,0.1")] public float SevereFalloffRange { get; set; } = 28.0f;
	[Export(PropertyHint.Range, "0,100,0.1")] public float BaseHitChance { get; set; } = 40.0f;
	[Export(PropertyHint.Range, "0,100,0.1")] public float Precision { get; set; } = 50.0f;
	[Export(PropertyHint.Range, "0,100,0.1")] public float RecoilControl { get; set; } = 50.0f;
	[Export(PropertyHint.Range, "0,100,0.1")] public float Handling { get; set; } = 50.0f;
	[Export(PropertyHint.Range, "0,100,0.1")] public float HipFireAccuracy { get; set; } = 50.0f;
	[Export(PropertyHint.Range, "5,180,0.1")] public float HipFireConeAngleDegrees { get; set; } = 60.0f;
	[Export(PropertyHint.Range, "1,120,0.1")] public float AimConeAngleDegrees { get; set; } = 18.0f;
	[Export] public bool CanAimWhileMoving { get; set; } = true;
	[Export(PropertyHint.Range, "0.1,5,0.01")] public float ReloadDuration { get; set; } = 1.6f;
	[Export(PropertyHint.Range, "0,100,0.1")] public float AimBaseBonus { get; set; } = 8.0f;
	[Export(PropertyHint.Range, "0,50,0.1")] public float MovingAimPenalty { get; set; } = 10.0f;
	[Export(PropertyHint.Range, "0,50,0.1")] public float TargetMovingPenalty { get; set; } = 20.0f;
	[Export(PropertyHint.Range, "0,50,0.1")] public float SevereRangeHitPenalty { get; set; } = 30.0f;
	[Export(PropertyHint.Range, "0,50,0.1")] public float FalloffRangeHitPenalty { get; set; } = 12.0f;
	[Export(PropertyHint.Range, "0,1,0.01")] public float FalloffRangeDamageMultiplier { get; set; } = 0.85f;
	[Export(PropertyHint.Range, "0,1,0.01")] public float SevereRangeDamageMultiplier { get; set; } = 0.55f;
	[Export(PropertyHint.Range, "0.1,5,0.01")] public float AimStaminaDrainPerSecond { get; set; } = 10.0f;
}
