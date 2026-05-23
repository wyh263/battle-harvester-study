namespace BattleHarvesterStudy.Combat;

public readonly struct SkillRequirementCheckResult
{
	public bool Satisfied { get; init; }
	public string FailureDetail { get; init; }

	public static SkillRequirementCheckResult Passed()
	{
		return new SkillRequirementCheckResult
		{
			Satisfied = true,
			FailureDetail = string.Empty
		};
	}

	public static SkillRequirementCheckResult Failed(string detail)
	{
		return new SkillRequirementCheckResult
		{
			Satisfied = false,
			FailureDetail = detail
		};
	}
}
