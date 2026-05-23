using Godot;

namespace BattleHarvesterStudy.Items;

public sealed class InstalledWeaponSkillState
{
	public InstalledWeaponSkillState(int slotIndex)
	{
		SlotIndex = Mathf.Max(0, slotIndex);
	}

	public int SlotIndex { get; }
	public SkillDefinition? Skill { get; private set; }
	public ItemDefinition? SourceItemDefinition { get; private set; }
	public ItemAcquisitionState SourceAcquisitionState { get; private set; }
	public int RemainingUses { get; private set; }
	public bool ConsumeUsesOnCast { get; private set; }
	public bool PermanentlyUnlocked { get; private set; }
	public bool CanBeLearned { get; private set; }
	public WeaponSkillUnlockConditionType UnlockConditionType { get; private set; }
	public int RequiredUseCount { get; private set; }
	public int RequiredKillCount { get; private set; }
	public int RequiredBossKillCount { get; private set; }
	public int CurrentUseCount { get; private set; }
	public int CurrentKillCount { get; private set; }
	public int CurrentBossKillCount { get; private set; }

	public bool IsEmpty => Skill == null;
	public bool CanCast => Skill != null && (PermanentlyUnlocked || !ConsumeUsesOnCast || RemainingUses > 0);

	public static InstalledWeaponSkillState CreateEmpty(int slotIndex)
	{
		return new InstalledWeaponSkillState(slotIndex);
	}

	public static InstalledWeaponSkillState CreateInstalled(
		int slotIndex,
		SkillDefinition skill,
		int remainingUses,
		ItemDefinition? sourceItemDefinition,
		ItemAcquisitionState sourceAcquisitionState,
		bool consumeUsesOnCast,
		bool canBeLearned,
		WeaponSkillUnlockConditionType unlockConditionType,
		int requiredUseCount,
		int requiredKillCount,
		int requiredBossKillCount)
	{
		InstalledWeaponSkillState state = new(slotIndex);
		state.Install(
			skill,
			remainingUses,
			sourceItemDefinition,
			sourceAcquisitionState,
			consumeUsesOnCast,
			canBeLearned,
			unlockConditionType,
			requiredUseCount,
			requiredKillCount,
			requiredBossKillCount);
		return state;
	}

	public static InstalledWeaponSkillState CreateLearned(int slotIndex, SkillDefinition skill)
	{
		return new InstalledWeaponSkillState(slotIndex)
		{
			Skill = skill,
			RemainingUses = 0,
			ConsumeUsesOnCast = false,
			PermanentlyUnlocked = true,
			CanBeLearned = false,
			UnlockConditionType = WeaponSkillUnlockConditionType.None,
			RequiredUseCount = 0,
			RequiredKillCount = 0,
			RequiredBossKillCount = 0,
			CurrentUseCount = 0,
			CurrentKillCount = 0,
			CurrentBossKillCount = 0,
		};
	}

	public static InstalledWeaponSkillState Restore(
		int slotIndex,
		SkillDefinition? skill,
		ItemDefinition? sourceItemDefinition,
		ItemAcquisitionState sourceAcquisitionState,
		int remainingUses,
		bool consumeUsesOnCast,
		bool permanentlyUnlocked,
		bool canBeLearned,
		WeaponSkillUnlockConditionType unlockConditionType,
		int requiredUseCount,
		int requiredKillCount,
		int requiredBossKillCount,
		int currentUseCount,
		int currentKillCount,
		int currentBossKillCount)
	{
		return new InstalledWeaponSkillState(slotIndex)
		{
			Skill = skill,
			SourceItemDefinition = sourceItemDefinition,
			SourceAcquisitionState = sourceAcquisitionState,
			RemainingUses = Mathf.Max(0, remainingUses),
			ConsumeUsesOnCast = consumeUsesOnCast,
			PermanentlyUnlocked = permanentlyUnlocked,
			CanBeLearned = canBeLearned,
			UnlockConditionType = unlockConditionType,
			RequiredUseCount = Mathf.Max(0, requiredUseCount),
			RequiredKillCount = Mathf.Max(0, requiredKillCount),
			RequiredBossKillCount = Mathf.Max(0, requiredBossKillCount),
			CurrentUseCount = Mathf.Max(0, currentUseCount),
			CurrentKillCount = Mathf.Max(0, currentKillCount),
			CurrentBossKillCount = Mathf.Max(0, currentBossKillCount),
		};
	}

	public void Install(
		SkillDefinition skill,
		int remainingUses,
		ItemDefinition? sourceItemDefinition,
		ItemAcquisitionState sourceAcquisitionState,
		bool consumeUsesOnCast,
		bool canBeLearned,
		WeaponSkillUnlockConditionType unlockConditionType,
		int requiredUseCount,
		int requiredKillCount,
		int requiredBossKillCount)
	{
		Skill = skill;
		RemainingUses = Mathf.Max(0, remainingUses);
		SourceItemDefinition = sourceItemDefinition;
		SourceAcquisitionState = sourceAcquisitionState;
		ConsumeUsesOnCast = consumeUsesOnCast;
		CanBeLearned = canBeLearned;
		UnlockConditionType = unlockConditionType;
		RequiredUseCount = Mathf.Max(0, requiredUseCount);
		RequiredKillCount = Mathf.Max(0, requiredKillCount);
		RequiredBossKillCount = Mathf.Max(0, requiredBossKillCount);
		CurrentUseCount = 0;
		CurrentKillCount = 0;
		CurrentBossKillCount = 0;
		PermanentlyUnlocked = false;
		RefreshPermanentUnlock();
	}

	public bool MatchesSkill(SkillDefinition? skill)
	{
		return skill != null && Skill?.SkillId == skill.SkillId;
	}

	public SkillDefinition? ResolveCastableSkill()
	{
		return CanCast ? Skill : null;
	}

	public bool RecordCast(out bool depleted)
	{
		depleted = false;
		if (!CanCast)
		{
			return false;
		}

		CurrentUseCount++;
		if (!PermanentlyUnlocked && ConsumeUsesOnCast && RemainingUses > 0)
		{
			RemainingUses--;
		}

		RefreshPermanentUnlock();
		depleted = !PermanentlyUnlocked && ConsumeUsesOnCast && RemainingUses <= 0;
		return true;
	}

	public ItemInstance? CreateRefundItem()
	{
		if (SourceItemDefinition == null)
		{
			return null;
		}

		ItemInstance item = new(SourceItemDefinition, 1, false, remainingUses: RemainingUses);
		item.SetAcquisitionState(SourceAcquisitionState);
		return item;
	}

	public void RecordKill(bool bossKill)
	{
		if (IsEmpty)
		{
			return;
		}

		if (bossKill)
		{
			CurrentBossKillCount++;
		}
		else
		{
			CurrentKillCount++;
		}

		RefreshPermanentUnlock();
	}

	public InstalledWeaponSkillState CreateCopy()
	{
		return new InstalledWeaponSkillState(SlotIndex)
		{
			Skill = Skill,
			SourceItemDefinition = SourceItemDefinition,
			SourceAcquisitionState = SourceAcquisitionState,
			RemainingUses = RemainingUses,
			ConsumeUsesOnCast = ConsumeUsesOnCast,
			PermanentlyUnlocked = PermanentlyUnlocked,
			CanBeLearned = CanBeLearned,
			UnlockConditionType = UnlockConditionType,
			RequiredUseCount = RequiredUseCount,
			RequiredKillCount = RequiredKillCount,
			RequiredBossKillCount = RequiredBossKillCount,
			CurrentUseCount = CurrentUseCount,
			CurrentKillCount = CurrentKillCount,
			CurrentBossKillCount = CurrentBossKillCount,
		};
	}

	private void RefreshPermanentUnlock()
	{
		if (!CanBeLearned || Skill == null)
		{
			return;
		}

		bool learned = UnlockConditionType switch
		{
			WeaponSkillUnlockConditionType.None => false,
			WeaponSkillUnlockConditionType.UseCount => CurrentUseCount >= RequiredUseCount,
			WeaponSkillUnlockConditionType.KillCount => CurrentKillCount >= RequiredKillCount,
			WeaponSkillUnlockConditionType.BossKillCount => CurrentBossKillCount >= RequiredBossKillCount,
			WeaponSkillUnlockConditionType.Composite => CurrentUseCount >= RequiredUseCount
				&& CurrentKillCount >= RequiredKillCount
				&& CurrentBossKillCount >= RequiredBossKillCount,
			_ => false
		};

		if (learned)
		{
			PermanentlyUnlocked = true;
		}
	}
}
