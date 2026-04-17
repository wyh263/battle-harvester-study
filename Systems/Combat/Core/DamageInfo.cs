using Godot;

namespace BattleHarvesterStudy;

public struct DamageInfo
{
	public int Amount;
	public Vector3 Knockback;
	public Node3D Attacker;
	public string AttackId;
	public bool CausesForcedMovement;
	public float ForcedMovementDuration;
	public int ForcedMovementPriority;

	public DamageInfo(
		int amount,
		Vector3 knockback,
		Node3D attacker,
		string attackId = "",
		bool causesForcedMovement = true,
		float forcedMovementDuration = 0.10f,
		int forcedMovementPriority = 10
	)
	{
		Amount = amount;
		Knockback = knockback;
		Attacker = attacker;
		AttackId = attackId;
		CausesForcedMovement = causesForcedMovement;
		ForcedMovementDuration = forcedMovementDuration;
		ForcedMovementPriority = forcedMovementPriority;
	}
}
