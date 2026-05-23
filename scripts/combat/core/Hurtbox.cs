using Godot;
using System;

namespace BattleHarvesterStudy.Combat;

public partial class Hurtbox : Area3D
{
	[Signal]
	public delegate void DamagedEventHandler(Node3D attacker, int amount, Vector3 knockback);

	public event Action<DamageInfo>? DamageTaken;

	public override void _Ready()
	{
		Monitoring = true;
		Monitorable = true;
	}

	public void TakeDamage(DamageInfo info)
	{
		string attackerName = info.Attacker != null ? info.Attacker.Name : "Unknown";
		GD.Print($"{Name} took {info.Amount} damage from {attackerName}");
		Node? componentsRoot = GetParent()?.GetNodeOrNull<Node>("Components");
		ArmorComponent? armor = componentsRoot?.GetNodeOrNull<ArmorComponent>("Armor");
		armor?.ApplyArmorDamage(info.ArmorAbsorbed);
		armor?.ApplyDurabilityDamage(info.DurabilityDamage);
		HealthComponent? health = componentsRoot?.GetNodeOrNull<HealthComponent>("Health");
		health?.ApplyDamage(info);
		DamageTaken?.Invoke(info);
		EmitSignal(SignalName.Damaged, info.Attacker, info.Amount, info.Knockback);
	}
}
