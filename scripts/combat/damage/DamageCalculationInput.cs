using BattleHarvesterStudy.Attributes;

namespace BattleHarvesterStudy.Combat;

public readonly struct DamageCalculationInput
{
	public required float BaseDamage { get; init; }
	public required float AttackPowerScaling { get; init; }
	public required float MagicPowerScaling { get; init; }
	public required DamageSchool School { get; init; }
	public required DamageDeliveryModel DeliveryModel { get; init; }
	public required bool CanCrit { get; init; }
	public required float SkillMultiplier { get; init; }
	public required float SkillPowerTier { get; init; }
	public required float DestructionMultiplier { get; init; }
	public required bool BypassArmorPoint { get; init; }
	public required WeaponEquipmentDefinition? ActiveWeapon { get; init; }
	public required float WeaponConditionDamageMultiplier { get; init; }
	public required StatsComponent? AttackerStats { get; init; }
	public required StatsComponent? DefenderStats { get; init; }
	public required ArmorComponent? DefenderArmor { get; init; }
}
