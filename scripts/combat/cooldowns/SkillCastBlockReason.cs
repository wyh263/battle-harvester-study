namespace BattleHarvesterStudy.Combat;

public enum SkillCastBlockReason
{
	None = 0,
	SkillCooldown = 1,
	CooldownGroup = 2,
	GlobalCooldown = 3,
	ResourceInsufficient = 4,
	RequirementNotMet = 5,
	ChainWindowClosed = 6,
}
