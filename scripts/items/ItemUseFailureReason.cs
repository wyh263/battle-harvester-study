namespace BattleHarvesterStudy.Items;

public enum ItemUseFailureReason
{
	None = 0,
	MissingItem = 1,
	NotUsable = 2,
	NoEffect = 3,
	SkillCastBlocked = 4,
	RepairBlocked = 5,
	WeaponSkillInstallBlocked = 6,
}
