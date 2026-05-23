using Godot;

namespace BattleHarvesterStudy.Combat;

[GlobalClass]
public partial class TargetStatusRequirement : SkillCastRequirement
{
	[Export]
	public string RequiredStatusId { get; set; } = string.Empty;

	public override SkillRequirementCheckResult Evaluate(SkillCastContext context)
	{
		if (!context.HasLockedTarget)
		{
			return SkillRequirementCheckResult.Failed("需要锁定目标");
		}

		if (string.IsNullOrWhiteSpace(RequiredStatusId))
		{
			return SkillRequirementCheckResult.Passed();
		}

		if (context.TargetHasStatus(RequiredStatusId))
		{
			return SkillRequirementCheckResult.Passed();
		}

		return SkillRequirementCheckResult.Failed($"目标需要状态 {RequiredStatusId}");
	}
}
