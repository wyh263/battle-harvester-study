using Godot;
using System;
using System.Collections.Generic;

namespace BattleHarvesterStudy.Attributes;

public partial class StatsComponent : Node
{
	public readonly struct StatModifier
	{
		public float Flat { get; init; }
		public float Multiplier { get; init; }
	}

	[Export]
	public float BaseMaxHealth { get; set; } = 100.0f;

	[Export]
	public float BaseAttackPower { get; set; } = 0.0f;

	[Export]
	public float BaseDefense { get; set; } = 0.0f;

	[Export(PropertyHint.Range, "0,1,0.01")]
	public float BaseCritChance { get; set; } = 0.05f;

	[Export(PropertyHint.Range, "1,5,0.01")]
	public float BaseCritDamage { get; set; } = 1.5f;

	[Export]
	public float BaseMoveSpeed { get; set; } = 5.0f;

	[Export]
	public float BaseRunMultiplier { get; set; } = 2.0f;

	[Export]
	public float BaseDashSpeed { get; set; } = 24.0f;

	[Export]
	public float BaseDashDuration { get; set; } = 0.18f;

	[Export]
	public float BaseDashCooldown { get; set; } = 0.35f;

	[Export]
	public float BaseDashInvulnerableDuration { get; set; } = 0.10f;

	[Export]
	public float BaseCooldownRate { get; set; } = 1.0f;

	[Export]
	public float BaseDropRate { get; set; } = 1.0f;

	[Export]
	public float BaseWeaponPenetrationTier { get; set; } = 0.0f;

	[Export]
	public float BaseArmorTier { get; set; } = 0.0f;

	[Export(PropertyHint.Range, "0,0.95,0.01")]
	public float BaseArmorAbsorbRate { get; set; } = 0.0f;

	[Export]
	public float BaseMagicPower { get; set; } = 0.0f;

	[Export]
	public float BaseMagicResistance { get; set; } = 0.0f;

	[Export(PropertyHint.Range, "0,100,0.01")]
	public float BaseCombatProficiency { get; set; } = 0.0f;

	[Export]
	public float BaseArmorPointMax { get; set; } = 0.0f;

	[Export(PropertyHint.Range, "1,9999,1")]
	public float BaseDefenseReductionK { get; set; } = 400.0f;

	public event Action? StatsChanged;

	private readonly Dictionary<StatType, Dictionary<string, StatModifier>> _statModifiers = [];

	public float RunMultiplier => GetValue(StatType.RunMultiplier);
	public float DashSpeed => GetValue(StatType.DashSpeed);
	public float DashDuration => GetValue(StatType.DashDuration);
	public float DashCooldown => GetValue(StatType.DashCooldown);
	public float DashInvulnerableDuration => GetValue(StatType.DashInvulnerableDuration);

	public float GetValue(StatType statType)
	{
		float flat = GetBaseValue(statType);
		float multiplier = 1.0f;

		if (_statModifiers.TryGetValue(statType, out Dictionary<string, StatModifier>? modifiers))
		{
			foreach (StatModifier modifier in modifiers.Values)
			{
				flat += modifier.Flat;
				multiplier *= modifier.Multiplier;
			}
		}

		return Mathf.Max(0.0f, flat * multiplier);
	}

	public float GetCurrentSpeed()
	{
		return GetValue(StatType.MoveSpeed);
	}

	public void SetStatModifier(string key, StatType statType, float flat = 0.0f, float multiplier = 1.0f)
	{
		if (!_statModifiers.TryGetValue(statType, out Dictionary<string, StatModifier>? modifiers))
		{
			modifiers = [];
			_statModifiers[statType] = modifiers;
		}

		modifiers[key] = new StatModifier
		{
			Flat = flat,
			Multiplier = multiplier
		};
		StatsChanged?.Invoke();
	}

	public void RemoveStatModifier(string key, StatType statType)
	{
		if (_statModifiers.TryGetValue(statType, out Dictionary<string, StatModifier>? modifiers)
			&& modifiers.Remove(key))
		{
			StatsChanged?.Invoke();
		}
	}

	public void SetMoveSpeedModifier(string key, float additive = 0.0f, float multiplier = 1.0f)
	{
		SetStatModifier(key, StatType.MoveSpeed, additive, multiplier);
	}

	public void RemoveMoveSpeedModifier(string key)
	{
		RemoveStatModifier(key, StatType.MoveSpeed);
	}

	public float GetDashSpeedMultiplier()
	{
		float baseMoveSpeed = GetValue(StatType.MoveSpeed);
		if (baseMoveSpeed <= 0.0f)
		{
			return 1.0f;
		}

		return GetValue(StatType.DashSpeed) / baseMoveSpeed;
	}

	private float GetBaseValue(StatType statType)
	{
		return statType switch
		{
			StatType.MaxHealth => BaseMaxHealth,
			StatType.AttackPower => BaseAttackPower,
			StatType.Defense => BaseDefense,
			StatType.CritChance => BaseCritChance,
			StatType.CritDamage => BaseCritDamage,
			StatType.MoveSpeed => BaseMoveSpeed,
			StatType.RunMultiplier => BaseRunMultiplier,
			StatType.DashSpeed => BaseDashSpeed,
			StatType.DashDuration => BaseDashDuration,
			StatType.DashCooldown => BaseDashCooldown,
			StatType.DashInvulnerableDuration => BaseDashInvulnerableDuration,
			StatType.CooldownRate => BaseCooldownRate,
			StatType.DropRate => BaseDropRate,
			StatType.WeaponPenetrationTier => BaseWeaponPenetrationTier,
			StatType.ArmorTier => BaseArmorTier,
			StatType.ArmorAbsorbRate => BaseArmorAbsorbRate,
			StatType.MagicPower => BaseMagicPower,
			StatType.MagicResistance => BaseMagicResistance,
			StatType.CombatProficiency => BaseCombatProficiency,
			StatType.ArmorPointMax => BaseArmorPointMax,
			StatType.DefenseReductionK => BaseDefenseReductionK,
			_ => 0.0f
		};
	}
}
