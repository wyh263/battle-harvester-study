using Godot;

namespace BattleHarvesterStudy.Combat;

[GlobalClass]
public abstract partial class SkillCastRequirement : Resource
{
	public abstract SkillRequirementCheckResult Evaluate(SkillCastContext context);
}
