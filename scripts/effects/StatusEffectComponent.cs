using Godot;
using System.Collections.Generic;

namespace BattleHarvesterStudy.Effects;

public partial class StatusEffectComponent : Node
	, IStatusQuerySource
{
	private sealed class BleedInstance
	{
		public required Node3D Source;
		public required string SkillId;
		public StatusPresentationDefinition? Presentation;
		public required int DamagePerTick;
		public required float TickIntervalSeconds;
		public required float TotalDurationSeconds;
		public float RemainingDuration;
		public float TimeUntilNextTick;
	}

	private sealed class SlowInstance
	{
		public required string SourceKey;
		public required Node3D Source;
		public required string SkillId;
		public StatusPresentationDefinition? Presentation;
		public required float Multiplier;
		public float RemainingDuration;
	}

	private Node3D? _owner;
	private Hurtbox? _hurtbox;
	private StatsComponent? _stats;
	private readonly List<BleedInstance> _bleeds = new();
	private readonly List<SlowInstance> _slows = new();

	public override void _Ready()
	{
		_owner = GetOwner<Node3D>();
		_hurtbox = _owner?.GetNodeOrNull<Hurtbox>("Hurtbox");
		_stats = _owner?.GetNodeOrNull<StatsComponent>("Components/Stats");
		SetPhysicsProcess(true);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_hurtbox != null && _bleeds.Count > 0)
		{
			for (int index = _bleeds.Count - 1; index >= 0; index--)
			{
				BleedInstance bleed = _bleeds[index];
				bleed.RemainingDuration -= (float)delta;
				bleed.TimeUntilNextTick -= (float)delta;

				while (bleed.TimeUntilNextTick <= 0.0f && bleed.RemainingDuration > 0.0f)
				{
					_hurtbox.TakeDamage(new DamageInfo(
						bleed.DamagePerTick,
						Vector3.Zero,
						bleed.Source,
						bleed.SkillId,
						false,
						0.0f,
						0
					));
					bleed.TimeUntilNextTick += Mathf.Max(0.05f, bleed.TickIntervalSeconds);
				}

				if (bleed.RemainingDuration <= 0.0f)
				{
					PublishStatusPhase(bleed.Source, bleed.SkillId, "bleed", StatusPresentationPhase.Expired, bleed.Presentation);
					_bleeds.RemoveAt(index);
				}
			}
		}

		if (_stats == null || _slows.Count == 0)
		{
			return;
		}

		for (int index = _slows.Count - 1; index >= 0; index--)
		{
			SlowInstance slow = _slows[index];
			slow.RemainingDuration -= (float)delta;
			if (slow.RemainingDuration <= 0.0f)
			{
				_stats.RemoveMoveSpeedModifier(slow.SourceKey);
				PublishStatusPhase(slow.Source, slow.SkillId, "slow", StatusPresentationPhase.Expired, slow.Presentation);
				_slows.RemoveAt(index);
			}
		}
	}

	public void ApplyBleed(
		Node3D source,
		string skillId,
		int damagePerTick,
		float tickIntervalSeconds,
		float totalDurationSeconds,
		StatusPresentationDefinition? presentation = null)
	{
		_bleeds.Add(new BleedInstance
		{
			Source = source,
			SkillId = skillId,
			Presentation = presentation,
			DamagePerTick = damagePerTick,
			TickIntervalSeconds = Mathf.Max(0.05f, tickIntervalSeconds),
			TotalDurationSeconds = Mathf.Max(0.05f, totalDurationSeconds),
			RemainingDuration = Mathf.Max(0.05f, totalDurationSeconds),
			TimeUntilNextTick = Mathf.Max(0.05f, tickIntervalSeconds)
		});
		PublishStatusPhase(source, skillId, "bleed", StatusPresentationPhase.Applied, presentation);
	}

	public void ApplySlow(
		Node3D source,
		string sourceSkillId,
		float slowPercent,
		float totalDurationSeconds,
		StatusPresentationDefinition? presentation = null)
	{
		if (_stats == null)
		{
			return;
		}

		float clampedSlowPercent = Mathf.Clamp(slowPercent, 0.0f, 0.95f);
		string sourceKey = $"slow_{source.GetInstanceId()}_{sourceSkillId}";
		_stats.SetMoveSpeedModifier(sourceKey, multiplier: 1.0f - clampedSlowPercent);

		for (int index = 0; index < _slows.Count; index++)
		{
			if (_slows[index].SourceKey != sourceKey)
			{
				continue;
			}

			_slows[index].Presentation = presentation;
			_slows[index].Multiplier = 1.0f - clampedSlowPercent;
			_slows[index].RemainingDuration = Mathf.Max(_slows[index].RemainingDuration, totalDurationSeconds);
			PublishStatusPhase(source, sourceSkillId, "slow", StatusPresentationPhase.Applied, presentation);
			return;
		}

		_slows.Add(new SlowInstance
		{
			SourceKey = sourceKey,
			Source = source,
			SkillId = sourceSkillId,
			Presentation = presentation,
			Multiplier = 1.0f - clampedSlowPercent,
			RemainingDuration = Mathf.Max(0.05f, totalDurationSeconds)
		});
		PublishStatusPhase(source, sourceSkillId, "slow", StatusPresentationPhase.Applied, presentation);
	}

	private void PublishStatusPhase(
		Node3D source,
		string skillId,
		string statusId,
		StatusPresentationPhase phase,
		StatusPresentationDefinition? presentation)
	{
		if (_owner == null)
		{
			return;
		}

		CombatPresentationEvents.PublishStatusPhase(source, _owner, skillId, statusId, phase, presentation);
	}

	public bool HasStatus(string statusId)
	{
		return GetStatusRemaining(statusId) > 0.0f;
	}

	public float GetStatusRemaining(string statusId)
	{
		string normalized = statusId.Trim().ToLowerInvariant();
		return normalized switch
		{
			"bleed" => GetLongestBleedRemaining(),
			"slow" => GetLongestSlowRemaining(),
			_ => 0.0f
		};
	}

	private float GetLongestBleedRemaining()
	{
		float remaining = 0.0f;
		foreach (BleedInstance bleed in _bleeds)
		{
			remaining = Mathf.Max(remaining, bleed.RemainingDuration);
		}

		return remaining;
	}

	private float GetLongestSlowRemaining()
	{
		float remaining = 0.0f;
		foreach (SlowInstance slow in _slows)
		{
			remaining = Mathf.Max(remaining, slow.RemainingDuration);
		}

		return remaining;
	}
}
