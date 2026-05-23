using Godot;
using System;

namespace BattleHarvesterStudy.Attributes;

public partial class HealthComponent : Node
{
	[Export]
	public NodePath StatsPath { get; set; } = new("../Stats");

	[Signal]
	public delegate void HealthChangedEventHandler(float currentHealth, float maxHealth);

	[Signal]
	public delegate void DiedEventHandler();

	public event Action<float, float>? HealthChangedOccurred;
	public event Action? DiedOccurred;

	[Export(PropertyHint.Range, "0,1,0.01")]
	public float StartingHealthRatio { get; set; } = 1.0f;

	public float CurrentHealth { get; private set; }
	public float MaxHealth => Mathf.Max(1.0f, _stats?.GetValue(StatType.MaxHealth) ?? 100.0f);
	public bool IsDead => CurrentHealth <= 0.0f;

	private StatsComponent? _stats;

	public override void _Ready()
	{
		_stats = ResolveStats();
		if (_stats != null)
		{
			_stats.StatsChanged += OnStatsChanged;
		}

		CurrentHealth = MaxHealth * Mathf.Clamp(StartingHealthRatio, 0.0f, 1.0f);
		EmitHealthChanged();
	}

	public override void _ExitTree()
	{
		if (_stats != null)
		{
			_stats.StatsChanged -= OnStatsChanged;
		}
	}

	public void ApplyDamage(DamageInfo info)
	{
		if (info.Amount <= 0 || IsDead)
		{
			return;
		}

		CurrentHealth = Mathf.Max(0.0f, CurrentHealth - info.Amount);
		EmitHealthChanged();
		if (IsDead)
		{
			DiedOccurred?.Invoke();
			EmitSignal(SignalName.Died);
		}
	}

	public void Restore(float amount)
	{
		if (amount <= 0.0f)
		{
			return;
		}

		CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0.0f, MaxHealth);
		EmitHealthChanged();
	}

	public void Refill()
	{
		CurrentHealth = MaxHealth;
		EmitHealthChanged();
	}

	private void OnStatsChanged()
	{
		CurrentHealth = Mathf.Clamp(CurrentHealth, 0.0f, MaxHealth);
		EmitHealthChanged();
	}

	private void EmitHealthChanged()
	{
		HealthChangedOccurred?.Invoke(CurrentHealth, MaxHealth);
		EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
	}

	private StatsComponent? ResolveStats()
	{
		StatsComponent? stats = GetNodeOrNull<StatsComponent>(StatsPath);
		if (stats != null)
		{
			return stats;
		}

		Node3D? owner = GetOwnerOrNull<Node3D>();
		return owner?.GetNodeOrNull<StatsComponent>("Components/Stats");
	}
}
