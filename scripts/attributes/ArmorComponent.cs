using Godot;
using System;

namespace BattleHarvesterStudy.Attributes;

public partial class ArmorComponent : Node
{
	[Export]
	public NodePath StatsPath { get; set; } = new("../Stats");

	[Signal]
	public delegate void ArmorChangedEventHandler(float currentArmor, float maxArmor);

	public event Action<float, float>? ArmorChangedOccurred;

	public float CurrentArmor { get; private set; }
	public float CurrentArmorPoint => CurrentArmor;
	public float CurrentMaxArmor { get; private set; }
	public float MaxArmor => Mathf.Max(0.0f, CurrentMaxArmor);
	public float MaxArmorPoint => MaxArmor;
	public bool HasArmor => MaxArmor > 0.0f;

	[Export(PropertyHint.Range, "0,9999,1")]
	public float MaxDurability { get; set; } = 100.0f;

	public float CurrentDurability { get; private set; }

	private StatsComponent? _stats;
	private float _lastKnownBaseMaxArmor;

	public override void _Ready()
	{
		_stats = ResolveStats();
		if (_stats != null)
		{
			_stats.StatsChanged += OnStatsChanged;
		}

		_lastKnownBaseMaxArmor = ResolveBaseMaxArmor();
		CurrentMaxArmor = _lastKnownBaseMaxArmor;
		CurrentArmor = CurrentMaxArmor;
		CurrentDurability = CurrentMaxArmor;
		EmitArmorChanged();
	}

	public override void _ExitTree()
	{
		if (_stats != null)
		{
			_stats.StatsChanged -= OnStatsChanged;
		}
	}

	public float ApplyArmorDamage(float amount)
	{
		if (amount <= 0.0f || CurrentArmor <= 0.0f)
		{
			return 0.0f;
		}

		float absorbed = Mathf.Min(CurrentArmor, amount);
		CurrentArmor -= absorbed;
		EmitArmorChanged();
		return absorbed;
	}

	public void Refill()
	{
		CurrentArmor = CurrentMaxArmor;
		CurrentDurability = CurrentMaxArmor;
		EmitArmorChanged();
	}

	public bool TryRepair(int repairTier)
	{
		int armorTier = Mathf.RoundToInt(_stats?.GetValue(StatType.ArmorTier) ?? 0.0f);
		if (armorTier <= 0 || repairTier < armorTier - 1 || CurrentMaxArmor <= 0.0f)
		{
			return false;
		}

		float efficiency = repairTier < armorTier ? 0.5f : 1.0f;
		CurrentMaxArmor *= 0.8f;
		CurrentArmor = CurrentMaxArmor * efficiency;
		CurrentDurability = CurrentMaxArmor;
		EmitArmorChanged();
		return true;
	}

	public float ApplyDurabilityDamage(float amount)
	{
		if (amount <= 0.0f || CurrentMaxArmor <= 0.0f)
		{
			return 0.0f;
		}

		float applied = Mathf.Min(CurrentMaxArmor, amount);
		CurrentMaxArmor -= applied;
		CurrentArmor = Mathf.Clamp(CurrentArmor, 0.0f, CurrentMaxArmor);
		CurrentDurability = CurrentMaxArmor;
		EmitArmorChanged();
		return applied;
	}

	private void OnStatsChanged()
	{
		float previousMaxArmor = _lastKnownBaseMaxArmor;
		float newMaxArmor = ResolveBaseMaxArmor();
		float delta = newMaxArmor - previousMaxArmor;

		if (delta > 0.0f)
		{
			CurrentMaxArmor += delta;
			CurrentArmor = Mathf.Min(CurrentMaxArmor, CurrentArmor + delta);
		}
		else
		{
			CurrentMaxArmor = Mathf.Max(0.0f, CurrentMaxArmor + delta);
			CurrentArmor = Mathf.Clamp(CurrentArmor, 0.0f, CurrentMaxArmor);
		}

		CurrentDurability = CurrentMaxArmor;
		_lastKnownBaseMaxArmor = newMaxArmor;
		EmitArmorChanged();
	}

	private float ResolveBaseMaxArmor()
	{
		float explicitMaxArmor = _stats?.GetValue(StatType.ArmorPointMax) ?? 0.0f;
		if (explicitMaxArmor > 0.0f)
		{
			return explicitMaxArmor;
		}

		int armorTier = Mathf.RoundToInt(_stats?.GetValue(StatType.ArmorTier) ?? 0.0f);
		return ResolveTierArmorPoint(armorTier);
	}

	private static float ResolveTierArmorPoint(int armorTier)
	{
		return armorTier switch
		{
			1 => 100.0f,
			2 => 250.0f,
			3 => 600.0f,
			4 => 1500.0f,
			>= 5 => 4000.0f,
			_ => 0.0f,
		};
	}

	private void EmitArmorChanged()
	{
		ArmorChangedOccurred?.Invoke(CurrentArmor, MaxArmor);
		EmitSignal(SignalName.ArmorChanged, CurrentArmor, MaxArmor);
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
