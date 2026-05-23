namespace BattleHarvesterStudy.Combat;

public interface ICooldownModifierSource
{
	float GetCooldownFlatReduction(SkillDefinition skill);
	float GetCooldownMultiplier(SkillDefinition skill);
	float GetGlobalCooldownFlatReduction(SkillDefinition skill);
	float GetGlobalCooldownMultiplier(SkillDefinition skill);
}
