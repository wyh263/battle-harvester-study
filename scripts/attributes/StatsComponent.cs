using Godot;
using System.Collections.Generic;

namespace BattleHarvesterStudy;

public partial class StatsComponent : Node
{
	public readonly struct MoveSpeedModifier
	{
		public float Additive { get; init; }
		public float Multiplier { get; init; }
	}

	[Export]
	public float BaseMoveSpeed { get; set; } = 5.0f;

	[Export]
	public float RunMultiplier { get; set; } = 2.0f;

	[Export]
	public float DashSpeed { get; set; } = 24.0f;

	[Export]
	public float DashDuration { get; set; } = 0.18f;

	[Export]
	public float DashCooldown { get; set; } = 0.35f;

	[Export]
	public float DashInvulnerableDuration { get; set; } = 0.10f;

	private readonly Dictionary<string, MoveSpeedModifier> _moveSpeedModifiers = new();

	public float GetCurrentSpeed()
	{
		float additive = 0.0f;
		float multiplier = 1.0f;

		foreach (MoveSpeedModifier modifier in _moveSpeedModifiers.Values)
		{
			additive += modifier.Additive;
			multiplier *= modifier.Multiplier;
		}

		return Mathf.Max(0.0f, (BaseMoveSpeed + additive) * multiplier);
	}

	public void SetMoveSpeedModifier(string key, float additive = 0.0f, float multiplier = 1.0f)
	{
		_moveSpeedModifiers[key] = new MoveSpeedModifier
		{
			Additive = additive,
			Multiplier = multiplier
		};
	}

	public void RemoveMoveSpeedModifier(string key)
	{
		_moveSpeedModifiers.Remove(key);
	}

	public float GetDashSpeedMultiplier()
	{
		if (BaseMoveSpeed <= 0.0f)
		{
			return 1.0f;
		}

		return DashSpeed / BaseMoveSpeed;
	}
}
