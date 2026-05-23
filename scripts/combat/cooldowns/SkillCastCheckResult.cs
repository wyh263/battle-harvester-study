namespace BattleHarvesterStudy.Combat;

public readonly struct SkillCastCheckResult
{
	public bool CanCast { get; init; }
	public SkillCastBlockReason BlockReason { get; init; }
	public float RemainingSkillCooldown { get; init; }
	public float RemainingGroupCooldown { get; init; }
	public float RemainingGlobalCooldown { get; init; }
	public float CurrentResource { get; init; }
	public float RequiredResource { get; init; }
	public string FailureDetail { get; init; }

	public float GetBlockingRemainingSeconds()
	{
		return BlockReason switch
		{
			SkillCastBlockReason.SkillCooldown => RemainingSkillCooldown,
			SkillCastBlockReason.CooldownGroup => RemainingGroupCooldown,
			SkillCastBlockReason.GlobalCooldown => RemainingGlobalCooldown,
			_ => 0.0f
		};
	}
}
