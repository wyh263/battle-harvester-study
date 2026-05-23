using Godot;

namespace BattleHarvesterStudy.Combat;

public struct DamageInfo
{
	public int Amount;
	public Vector3 Knockback;
	public Node3D Attacker;
	public string AttackId;
	public bool CausesForcedMovement;
	public float ForcedMovementDuration;
	public int ForcedMovementPriority;
	public int InterruptStrength;
	public bool IgnoresInterruptArmor;
	public int BaseAmount;
	public bool WasCritical;
	public int RawAmount;
	public int TierAdjustedAmount;
	public int ArmorAbsorbed;
	public int PostArmorAmount;
	public int TierDelta;
	public DamageSchool School;
	public DamageDeliveryModel DeliveryModel;
	public int DurabilityDamage;
	public float PenetrationEffectiveness;
	public float ArmorLossResistance;
	public float EffectiveAbsorbRate;

	public DamageInfo(
		int amount,
		Vector3 knockback,
		Node3D attacker,
		string attackId = "",
		bool causesForcedMovement = true,
		float forcedMovementDuration = 0.10f,
		int forcedMovementPriority = 10,
		int interruptStrength = 1,
		bool ignoresInterruptArmor = false,
		int baseAmount = 0,
		bool wasCritical = false,
		int rawAmount = 0,
		int tierAdjustedAmount = 0,
		int armorAbsorbed = 0,
		int postArmorAmount = 0,
		int tierDelta = 0,
		DamageSchool school = DamageSchool.Physical,
		DamageDeliveryModel deliveryModel = DamageDeliveryModel.Direct,
		int durabilityDamage = 0,
		float penetrationEffectiveness = 1.0f,
		float armorLossResistance = 1.0f,
		float effectiveAbsorbRate = 0.0f
	)
	{
		Amount = amount;
		Knockback = knockback;
		Attacker = attacker;
		AttackId = attackId;
		CausesForcedMovement = causesForcedMovement;
		ForcedMovementDuration = forcedMovementDuration;
		ForcedMovementPriority = forcedMovementPriority;
		InterruptStrength = interruptStrength;
		IgnoresInterruptArmor = ignoresInterruptArmor;
		BaseAmount = baseAmount <= 0 ? amount : baseAmount;
		WasCritical = wasCritical;
		RawAmount = rawAmount <= 0 ? BaseAmount : rawAmount;
		TierAdjustedAmount = tierAdjustedAmount <= 0 ? RawAmount : tierAdjustedAmount;
		ArmorAbsorbed = Mathf.Max(0, armorAbsorbed);
		PostArmorAmount = postArmorAmount <= 0 ? amount : postArmorAmount;
		TierDelta = tierDelta;
		School = school;
		DeliveryModel = deliveryModel;
		DurabilityDamage = Mathf.Max(0, durabilityDamage);
		PenetrationEffectiveness = penetrationEffectiveness;
		ArmorLossResistance = armorLossResistance;
		EffectiveAbsorbRate = effectiveAbsorbRate;
	}
}
