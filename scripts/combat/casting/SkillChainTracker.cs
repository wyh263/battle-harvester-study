using Godot;

namespace BattleHarvesterStudy.Combat;

public partial class SkillChainTracker : Node
{
	private double _elapsedSeconds;

	public string LastCastSkillId { get; private set; } = string.Empty;
	public double LastCastTimeSeconds { get; private set; } = double.NegativeInfinity;
	public string LastHitSkillId { get; private set; } = string.Empty;
	public double LastHitTimeSeconds { get; private set; } = double.NegativeInfinity;
	public SkillDefinition? CurrentSkill { get; private set; }
	public SkillPresentationPhase? CurrentPhase { get; private set; }
	public bool CurrentSkillHitConfirmed { get; private set; }
	public bool IsSkillActive => CurrentSkill != null;

	public override void _Ready()
	{
		SetProcess(true);
	}

	public override void _Process(double delta)
	{
		_elapsedSeconds += delta;
	}

	public void RecordCast(SkillDefinition skill)
	{
		LastCastSkillId = skill.SkillId;
		LastCastTimeSeconds = _elapsedSeconds;
	}

	public void BeginSkill(SkillDefinition skill)
	{
		CurrentSkill = skill;
		CurrentPhase = SkillPresentationPhase.Startup;
		CurrentSkillHitConfirmed = false;
	}

	public void UpdatePhase(SkillPresentationPhase phase)
	{
		if (CurrentSkill == null)
		{
			return;
		}

		CurrentPhase = phase;
		if (phase is SkillPresentationPhase.Completed or SkillPresentationPhase.Cancelled)
		{
			CurrentSkill = null;
			CurrentPhase = null;
			CurrentSkillHitConfirmed = false;
		}
	}

	public void RecordHit(HitResult hitResult)
	{
		LastHitSkillId = hitResult.Skill.SkillId;
		LastHitTimeSeconds = _elapsedSeconds;
		if (CurrentSkill != null && CurrentSkill.SkillId == hitResult.Skill.SkillId)
		{
			CurrentSkillHitConfirmed = true;
		}
	}

	public bool WasSkillCastRecently(string skillId, float maxElapsedSeconds)
	{
		return LastCastSkillId == skillId && (_elapsedSeconds - LastCastTimeSeconds) <= maxElapsedSeconds;
	}

	public bool WasSkillHitRecently(string skillId, float maxElapsedSeconds)
	{
		return LastHitSkillId == skillId && (_elapsedSeconds - LastHitTimeSeconds) <= maxElapsedSeconds;
	}

	public bool CanBufferNextSkill()
	{
		if (CurrentSkill == null || CurrentPhase == null)
		{
			return true;
		}

		return CurrentSkill.CanBufferNextSkill(CurrentPhase.Value, CurrentSkillHitConfirmed);
	}

	public bool CanDashCancelCurrentSkill()
	{
		if (CurrentSkill == null || CurrentPhase == null)
		{
			return false;
		}

		return CurrentSkill.CanDashCancel(CurrentPhase.Value);
	}

	public bool CanInterruptCurrentSkill(DamageInfo info)
	{
		if (CurrentSkill == null || CurrentPhase == null)
		{
			return false;
		}

		if (!CurrentSkill.CanBeInterrupted(CurrentPhase.Value))
		{
			return false;
		}

		if (info.IgnoresInterruptArmor)
		{
			return true;
		}

		return info.InterruptStrength > CurrentSkill.GetInterruptArmor(CurrentPhase.Value);
	}

	public string GetBufferBlockDetail()
	{
		if (CurrentSkill == null || CurrentPhase == null)
		{
			return string.Empty;
		}

		if (CurrentSkill.RequireHitConfirmForBuffer && !CurrentSkillHitConfirmed)
		{
			return $"BUFFER NEEDS HIT {CurrentSkill.SkillId}";
		}

		return $"连段窗口已关闭 {CurrentPhase.Value}";
	}
}
