using Godot;
using System.Collections.Generic;

namespace BattleHarvesterStudy;

public partial class StatusEffectComponent : Node
{
	private sealed class BleedInstance
	{
		public required Node3D Source;
		public required string SkillId;
		public required int DamagePerTick;
		public required float TickIntervalSeconds;
		public required float TotalDurationSeconds;
		public float RemainingDuration;
		public float TimeUntilNextTick;
	}

	private sealed class SlowInstance
	{
		public required string SourceKey;
		public required float Multiplier;
		public float RemainingDuration;
	}

	private Hurtbox? _hurtbox;
	private StatsComponent? _stats;
	private readonly List<BleedInstance> _bleeds = new();
	private readonly List<SlowInstance> _slows = new();

	public override void _Ready()
	{
		Node3D? owner = GetOwner<Node3D>();
		_hurtbox = owner?.GetNodeOrNull<Hurtbox>("Hurtbox");
		_stats = owner?.GetNodeOrNull<StatsComponent>("Components/Stats");
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
				_slows.RemoveAt(index);
			}
		}
	}

	public void ApplyBleed(Node3D source, string skillId, int damagePerTick, float tickIntervalSeconds, float totalDurationSeconds)
	{
		_bleeds.Add(new BleedInstance
		{
			Source = source,
			SkillId = skillId,
			DamagePerTick = damagePerTick,
			TickIntervalSeconds = Mathf.Max(0.05f, tickIntervalSeconds),
			TotalDurationSeconds = Mathf.Max(0.05f, totalDurationSeconds),
			RemainingDuration = Mathf.Max(0.05f, totalDurationSeconds),
			TimeUntilNextTick = Mathf.Max(0.05f, tickIntervalSeconds)
		});
	}

	public void ApplySlow(string sourceSkillId, float slowPercent, float totalDurationSeconds)
	{
		if (_stats == null)
		{
			return;
		}

		float clampedSlowPercent = Mathf.Clamp(slowPercent, 0.0f, 0.95f);
		string sourceKey = $"slow_{sourceSkillId}";
		_stats.SetMoveSpeedModifier(sourceKey, multiplier: 1.0f - clampedSlowPercent);

		for (int index = 0; index < _slows.Count; index++)
		{
			if (_slows[index].SourceKey != sourceKey)
			{
				continue;
			}

			_slows[index].Multiplier = 1.0f - clampedSlowPercent;
			_slows[index].RemainingDuration = Mathf.Max(_slows[index].RemainingDuration, totalDurationSeconds);
			return;
		}

		_slows.Add(new SlowInstance
		{
			SourceKey = sourceKey,
			Multiplier = 1.0f - clampedSlowPercent,
			RemainingDuration = Mathf.Max(0.05f, totalDurationSeconds)
		});
	}
}
