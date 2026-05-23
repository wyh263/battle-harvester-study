using Godot;
namespace BattleHarvesterStudy.Combat;

[GlobalClass]
public partial class PreviousSkillRequirement : SkillCastRequirement
{
	[Export]
	public string[] RequiredSkillIds { get; set; } = [];

	[Export]
	public bool RequireHitConfirm { get; set; }

	[Export(PropertyHint.Range, "0,10,0.01")]
	public float MaxElapsedSeconds { get; set; } = 1.5f;

	public override SkillRequirementCheckResult Evaluate(SkillCastContext context)
	{
		if (context.ChainTracker == null)
		{
			return SkillRequirementCheckResult.Failed("NO CHAIN TRACKER");
		}

		if (RequiredSkillIds.Length == 0)
		{
			return SkillRequirementCheckResult.Passed();
		}

		foreach (string skillId in RequiredSkillIds)
		{
			bool matched = RequireHitConfirm
				? context.ChainTracker.WasSkillHitRecently(skillId, MaxElapsedSeconds)
				: context.ChainTracker.WasSkillCastRecently(skillId, MaxElapsedSeconds);
			if (matched)
			{
				return SkillRequirementCheckResult.Passed();
			}
		}

		string mode = RequireHitConfirm ? "HIT" : "CAST";
		string joinedSkillIds = string.Join("/", RequiredSkillIds);
		return SkillRequirementCheckResult.Failed($"NEEDS {mode} {joinedSkillIds}");
	}
}
