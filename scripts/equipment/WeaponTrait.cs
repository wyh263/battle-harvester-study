namespace BattleHarvesterStudy.Equipment;

[System.Flags]
public enum WeaponTrait
{
	None = 0,
	RecoveryPunish = 1 << 0,
	HeavySwing = 1 << 1,
	Bleed = 1 << 2,
	Execution = 1 << 3,
	MultiHit = 1 << 4,
	Sustained = 1 << 5,
	ArmorBreak = 1 << 6,
	Reach = 1 << 7,
	Bypass = 1 << 8,
}
