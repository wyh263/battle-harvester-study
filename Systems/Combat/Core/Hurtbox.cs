using Godot;
using System;

namespace BattleHarvesterStudy;

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
		DamageTaken?.Invoke(info);
		EmitSignal(SignalName.Damaged, info.Attacker, info.Amount, info.Knockback);
	}
}
