namespace BattleHarvesterStudy.Equipment;

[System.Flags]
public enum WeaponArchetype
{
	None = 0,
	Sharp = 1 << 0,
	Blunt = 1 << 1,
	Projectile = 1 << 2,
	Flexible = 1 << 3,
	Mechanism = 1 << 4,
}
