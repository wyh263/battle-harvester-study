namespace BattleHarvesterStudy.Equipment;

[System.Flags]
public enum WeaponFamily
{
	None = 0,
	Sword = 1 << 0,
	Katana = 1 << 1,
	Greatsword = 1 << 2,
	Dagger = 1 << 3,
	Spear = 1 << 4,
	Hammer = 1 << 5,
	Axe = 1 << 6,
	Mace = 1 << 7,
	ThrowingKnife = 1 << 8,
	BoltLauncher = 1 << 9,
	Whip = 1 << 10,
	ChainBlade = 1 << 11,
	Drill = 1 << 12,
	Flamethrower = 1 << 13,
	Shotgun = 1 << 14,
	Rifle = 1 << 15,
	SniperRifle = 1 << 16,
}
