using Godot;
using System.Collections.Generic;
using System.Linq;
using BattleHarvesterStudy.Combat.Firearms;

namespace BattleHarvesterStudy.Presentation;

public static class EquipmentTextFormatter
{
	public static string GetSlotDisplayName(EquipmentSlotType slotType)
	{
		bool chinese = UiText.CurrentLocale == UiText.DefaultLocale;
		return slotType switch
		{
			EquipmentSlotType.WeaponSlot1 => chinese ? "\u6b66\u5668\u69fd 1" : "Weapon Slot 1",
			EquipmentSlotType.WeaponSlot2 => chinese ? "\u6b66\u5668\u69fd 2" : "Weapon Slot 2",
			EquipmentSlotType.Gloves => chinese ? "\u624b\u5957" : "Gloves",
			EquipmentSlotType.Armor => chinese ? "\u62a4\u7532" : "Armor",
			EquipmentSlotType.Shoes => chinese ? "\u978b\u5b50" : "Shoes",
			EquipmentSlotType.Item1 => chinese ? "\u9053\u5177\u69fd 1" : "Item Slot 1",
			EquipmentSlotType.Item2 => chinese ? "\u9053\u5177\u69fd 2" : "Item Slot 2",
			EquipmentSlotType.Item3 => chinese ? "\u9053\u5177\u69fd 3" : "Item Slot 3",
			_ => chinese ? "\u672a\u77e5\u69fd\u4f4d" : "Unknown Slot",
		};
	}

	public static string GetStatDisplayName(StatType statType)
	{
		bool chinese = UiText.CurrentLocale == UiText.DefaultLocale;
		return statType switch
		{
			StatType.MaxHealth => chinese ? "\u6700\u5927\u751f\u547d" : "Max Health",
			StatType.AttackPower => chinese ? "\u653b\u51fb\u529b" : "Attack Power",
			StatType.Defense => chinese ? "\u9632\u5fa1" : "Defense",
			StatType.CritChance => chinese ? "\u66b4\u51fb\u7387" : "Crit Chance",
			StatType.CritDamage => chinese ? "\u66b4\u51fb\u4f24\u5bb3" : "Crit Damage",
			StatType.MoveSpeed => chinese ? "\u79fb\u52a8\u901f\u5ea6" : "Move Speed",
			StatType.RunMultiplier => chinese ? "\u5954\u8dd1\u500d\u7387" : "Run Multiplier",
			StatType.DashSpeed => chinese ? "\u95ea\u907f\u901f\u5ea6" : "Dash Speed",
			StatType.DashDuration => chinese ? "\u95ea\u907f\u65f6\u957f" : "Dash Duration",
			StatType.DashCooldown => chinese ? "\u95ea\u907f\u51b7\u5374" : "Dash Cooldown",
			StatType.DashInvulnerableDuration => chinese ? "\u65e0\u654c\u65f6\u957f" : "Invulnerable Duration",
			StatType.CooldownRate => chinese ? "\u51b7\u5374\u500d\u7387" : "Cooldown Rate",
			StatType.DropRate => chinese ? "\u6389\u843d\u500d\u7387" : "Drop Rate",
			StatType.WeaponPenetrationTier => chinese ? "\u6b66\u5668\u7a7f\u900f\u7b49\u7ea7" : "Weapon Penetration Tier",
			StatType.ArmorTier => chinese ? "\u62a4\u7532\u7b49\u7ea7" : "Armor Tier",
			StatType.ArmorAbsorbRate => chinese ? "\u62a4\u7532\u62e6\u622a\u7387" : "Armor Absorb Rate",
			StatType.MagicPower => chinese ? "\u6cd5\u672f\u5f3a\u5ea6" : "Magic Power",
			StatType.MagicResistance => chinese ? "\u6cd5\u672f\u6297\u6027" : "Magic Resistance",
			StatType.CombatProficiency => chinese ? "\u6218\u6597\u7d20\u517b" : "Combat Proficiency",
			StatType.ArmorPointMax => chinese ? "\u62a4\u7532 AP \u4e0a\u9650" : "Armor AP Max",
			StatType.DefenseReductionK => chinese ? "\u9632\u5fa1 K \u503c" : "Defense K",
			_ => statType.ToString(),
		};
	}

	public static string GetBooleanText(bool value)
	{
		if (UiText.CurrentLocale == UiText.DefaultLocale)
		{
			return value ? "\u662f" : "\u5426";
		}

		return value ? "Yes" : "No";
	}

	public static string GetUnequippedText()
	{
		return UiText.CurrentLocale == UiText.DefaultLocale ? "\u672a\u88c5\u5907" : "Unequipped";
	}

	public static string FormatAllowedSlots(EquipmentDefinition? equipment)
	{
		if (equipment == null || equipment.AllowedSlots.Count == 0)
		{
			return "-";
		}

		List<string> slots = [];
		foreach (EquipmentSlotType slotType in equipment.AllowedSlots)
		{
			slots.Add(GetSlotDisplayName(slotType));
		}

		return string.Join(", ", slots.Distinct());
	}

	public static string FormatArchetype(EquipmentDefinition? equipment)
	{
		if (equipment?.Archetype == null)
		{
			return "-";
		}

		if (string.IsNullOrWhiteSpace(equipment.DisplayVariant))
		{
			return equipment.Archetype.DisplayName;
		}

		return $"{equipment.Archetype.DisplayName} / {equipment.DisplayVariant}";
	}

	public static string GetArmorClassDisplayName(ArmorClass armorClass)
	{
		bool chinese = UiText.CurrentLocale == UiText.DefaultLocale;
		return armorClass switch
		{
			ArmorClass.Light => chinese ? "轻甲" : "Light",
			ArmorClass.Medium => chinese ? "中甲" : "Medium",
			ArmorClass.Heavy => chinese ? "重甲" : "Heavy",
			_ => armorClass.ToString(),
		};
	}

	public static bool IsChestArmor(EquipmentDefinition? equipment)
	{
		return equipment is ArmorEquipmentDefinition || equipment != null && equipment.AllowsSlot(EquipmentSlotType.Armor);
	}

	public static bool IsWeapon(EquipmentDefinition? equipment)
	{
		if (equipment == null)
		{
			return false;
		}

		return equipment is WeaponEquipmentDefinition
			|| equipment.AllowsSlot(EquipmentSlotType.WeaponSlot1)
			|| equipment.AllowsSlot(EquipmentSlotType.WeaponSlot2);
	}

	public static float GetArmorValue(EquipmentDefinition? equipment)
	{
		if (equipment is ArmorEquipmentDefinition armor)
		{
			return Mathf.Max(0.0f, armor.MaxArmorPoint);
		}

		if (equipment == null)
		{
			return 0.0f;
		}

		float armorPoint = 0.0f;
		foreach (EquipmentStatModifierDefinition modifier in equipment.GetResolvedStatModifiers())
		{
			if (modifier.StatType == StatType.ArmorPointMax)
			{
				armorPoint += modifier.Flat;
			}
		}

		return Mathf.Max(0.0f, armorPoint);
	}

	public static float GetArmorAbsorbRate(EquipmentDefinition? equipment)
	{
		if (equipment == null)
		{
			return 0.0f;
		}

		if (equipment is ArmorEquipmentDefinition armor)
		{
			return Mathf.Clamp(armor.BaseAbsorbRate, 0.0f, 0.95f);
		}

		float absorbRate = 0.0f;
		foreach (EquipmentStatModifierDefinition modifier in equipment.GetResolvedStatModifiers())
		{
			if (modifier.StatType == StatType.ArmorAbsorbRate)
			{
				absorbRate += modifier.Flat;
			}
		}

		return Mathf.Clamp(absorbRate, 0.0f, 0.95f);
	}

	public static string FormatEquipmentDetails(EquipmentDefinition? equipment)
	{
		return FormatEquipmentDetails(null, equipment);
	}

	public static string FormatEquipmentDetails(ItemInstance? item)
	{
		return FormatEquipmentDetails(item, item?.Definition.Equipment);
	}

	private static string FormatEquipmentDetails(ItemInstance? item, EquipmentDefinition? equipment)
	{
		if (equipment == null)
		{
			return UiText.Resolve(UiTextKeys.Inventory.DetailsEquipmentNone);
		}

		List<string> lines = [];
		bool chinese = UiText.CurrentLocale == UiText.DefaultLocale;
		lines.Add(chinese ? "\u88c5\u5907\u4fe1\u606f" : "Equipment Info");
		lines.Add($"{(chinese ? "\u6837\u5f0f" : "Archetype")}  {FormatArchetype(equipment)}");
		lines.Add($"{(chinese ? "\u53ef\u7528\u69fd\u4f4d" : "Allowed Slots")}  {FormatAllowedSlots(equipment)}");

		if (equipment.GripType == EquipmentGripType.TwoHanded)
		{
			lines.Add($"{(chinese ? "\u53cc\u624b\u5360\u7528" : "Two-Handed")}  {GetBooleanText(true)}");
		}

		if (equipment is WeaponEquipmentDefinition weapon)
		{
			lines.Add($"{(chinese ? "\u6b66\u5668\u5927\u7c7b" : "Weapon Archetype")}  {WeaponClassificationUtility.GetArchetypeName(weapon.WeaponArchetypeId, chinese)}");
			lines.Add($"{(chinese ? "\u6b66\u5668\u5bb6\u65cf" : "Weapon Family")}  {WeaponClassificationUtility.GetFamilyName(weapon.WeaponFamilyId, chinese)}");
			lines.Add($"{(chinese ? "\u6b66\u5668\u7279\u6027" : "Weapon Traits")}  {WeaponClassificationUtility.FormatTraits(weapon.WeaponTraitMask, chinese)}");
			lines.Add($"{(chinese ? "\u6b66\u5668\u6280\u80fd\u69fd" : "Weapon Skill Slots")}  {weapon.GetWeaponSkillSlotCount()}");
			if (weapon.AllowedWeaponSkillTags.Count > 0)
			{
				lines.Add($"{(chinese ? "\u53ef\u88c5\u6280\u80fd\u6807\u7b7e" : "Allowed Skill Tags")}  {string.Join(", ", weapon.AllowedWeaponSkillTags)}");
			}
			lines.Add($"{(chinese ? "\u4f24\u5bb3\u500d\u7387" : "Damage Multiplier")}  x{weapon.BaseMultiplier:0.##}");
			lines.Add($"PT  {weapon.GetBasePenetrationTier():0.##}");
			lines.Add($"DM  x{weapon.DestructionMultiplierBias:0.##}");
			lines.Add($"{(chinese ? "\u7ed5\u8fc7 AP" : "Bypass AP")}  {GetBooleanText(weapon.GrantsBypassArmorPoint || weapon.HasTrait(WeaponTrait.Bypass))}");
			lines.Add($"{(chinese ? "\u66b4\u51fb" : "Crit")}  {weapon.CritRate * 100.0f:0.#}% / x{weapon.CritDamage:0.##}");
			lines.Add($"{(chinese ? "\u8010\u4e45" : "Durability")}  {(item?.CurrentDurability ?? weapon.MaxDurability):0.#}/{(item?.CurrentMaxDurability ?? weapon.MaxDurability):0.#}");
			lines.Add($"{(chinese ? "\u7834\u635f\u4f24\u5bb3" : "Broken Damage")}  x{weapon.BrokenDamageMultiplier:0.##}");
			lines.Add($"{(chinese ? "\u653b\u51fb\u7ec4" : "Move Set")}  {ContentTextFormatter.GetMoveSetDisplayName(weapon.MoveSet)}");
			if (weapon is FirearmWeaponDefinition firearm)
			{
				FirearmResolvedStats resolved = FirearmStatResolver.Resolve(firearm, item);
				lines.Add($"{(chinese ? "枪型" : "Gun Type")}  {WeaponClassificationUtility.GetFamilyName(firearm.WeaponFamilyId, chinese)}");
				lines.Add($"{(chinese ? "单发伤害" : "Pellet Damage")}  {resolved.BaseDamagePerPellet}");
				lines.Add($"{(chinese ? "弹丸数" : "Pellets")}  {resolved.PelletCount}");
				lines.Add($"{(chinese ? "射速" : "Fire Rate")}  {resolved.FireRate:0.##}/s");
				lines.Add($"{(chinese ? "弹匣" : "Magazine")}  {(item?.CurrentMagazineAmmo ?? resolved.MagazineCapacity)}/{resolved.MagazineCapacity}");
				lines.Add($"{(chinese ? "有效射程" : "Effective Range")}  {resolved.EffectiveRange:0.#}");
				lines.Add($"{(chinese ? "重衰减距离" : "Severe Falloff")}  {resolved.SevereFalloffRange:0.#}");
				lines.Add($"{(chinese ? "基础命中" : "Base Hit")}  {resolved.BaseHitChance:0.#}%");
				lines.Add($"{(chinese ? "精度" : "Precision")}  {resolved.Precision:0.#}");
				lines.Add($"{(chinese ? "后坐控制" : "Recoil Ctrl")}  {resolved.RecoilControl:0.#}");
				lines.Add($"{(chinese ? "操控" : "Handling")}  {resolved.Handling:0.#}");
				lines.Add($"{(chinese ? "腰射精度" : "Hip Fire")}  {resolved.HipFireAccuracy:0.#}");
				lines.Add($"{(chinese ? "射击模式" : "Fire Mode")}  {FormatFireMode(resolved.FireMode, chinese)}");
			}
			if (item?.HasWeaponSkillSlots == true)
			{
				for (int index = 0; index < item.WeaponSkillSlots.Count; index++)
				{
					InstalledWeaponSkillState state = item.WeaponSkillSlots[index];
					string skillText = state.Skill == null
						? (chinese ? "\u7a7a" : "Empty")
						: state.PermanentlyUnlocked
							? $"{ContentTextFormatter.GetSkillDisplayName(state.Skill)} {(chinese ? "(\u5df2\u4e60\u5f97)" : "(Learned)")}"
							: $"{ContentTextFormatter.GetSkillDisplayName(state.Skill)} {(chinese ? $"(\u6b21\u6570 {state.RemainingUses})" : $"({state.RemainingUses} uses)")}";
					lines.Add($"{(chinese ? "\u6280\u80fd\u69fd" : "Skill Slot")} {index + 1}  {skillText}");
				}
			}
		}

		if (equipment is ArmorEquipmentDefinition armor)
		{
			lines.Add($"{(chinese ? "\u62a4\u7532\u7c7b\u578b" : "Armor Class")}  {GetArmorClassDisplayName(armor.ArmorClass)}");
			lines.Add($"AT  {armor.ArmorTier}");
			lines.Add($"AP  {(item?.CurrentArmorPoint ?? armor.MaxArmorPoint):0.#}/{(item?.CurrentMaxArmorPoint ?? armor.MaxArmorPoint):0.#}");
			lines.Add($"{(chinese ? "\u7ef4\u4fee\u8870\u51cf" : "Repair Decay")}  x{armor.RepairDecayMultiplier:0.##}");
		}

		if (equipment is GloveEquipmentDefinition gloves)
		{
			lines.Add($"{(chinese ? "\u653b\u51fb\u529b" : "Attack Power")}  +{gloves.AttackPower:0.#}");
		}

		float armorValue = GetArmorValue(equipment);
		if (armorValue > 0)
		{
			lines.Add($"{(chinese ? "AP \u4e0a\u9650" : "AP Max")}  {armorValue:0.#}");
		}

		float armorAbsorbRate = GetArmorAbsorbRate(equipment);
		if (armorAbsorbRate > 0.0f)
		{
			lines.Add($"{(chinese ? "\u57fa\u7840\u62e6\u622a\u7387" : "Base Absorb Rate")}  {armorAbsorbRate * 100.0f:0.#}%");
		}

		string modifiers = FormatStatModifiers(equipment);
		if (modifiers != "-")
		{
			lines.Add(chinese ? "\u5c5e\u6027" : "Modifiers");
			lines.Add(modifiers);
		}

		return string.Join("\n", lines);
	}

	public static string FormatStatModifiers(EquipmentDefinition? equipment)
	{
		if (equipment == null)
		{
			return "-";
		}

		Godot.Collections.Array<EquipmentStatModifierDefinition> resolvedModifiers = equipment.GetResolvedStatModifiers();
		if (resolvedModifiers.Count == 0)
		{
			return "-";
		}

		List<string> lines = [];
		foreach (EquipmentStatModifierDefinition modifier in resolvedModifiers)
		{
			string? line = FormatStatModifier(equipment, modifier);
			if (!string.IsNullOrWhiteSpace(line))
			{
				lines.Add(line);
			}
		}

		return lines.Count == 0 ? "-" : string.Join("\n", lines);
	}

	public static string FormatRollBands(EquipmentDefinition? equipment)
	{
		if (equipment?.Archetype == null || equipment.Archetype.StatRollBands.Count == 0)
		{
			return "-";
		}

		List<string> lines = [];
		foreach (EquipmentStatRollBandDefinition rollBand in equipment.Archetype.StatRollBands)
		{
			string statName = GetStatDisplayName(rollBand.StatType);
			string flatText = $"{rollBand.MinFlat:0.##}~{rollBand.MaxFlat:0.##}";
			string multiplierText = $"{rollBand.MinMultiplier:0.##}~{rollBand.MaxMultiplier:0.##}";
			if (Mathf.IsEqualApprox(rollBand.MinMultiplier, 1.0f) && Mathf.IsEqualApprox(rollBand.MaxMultiplier, 1.0f))
			{
				lines.Add($"{statName} {flatText}");
			}
			else
			{
				lines.Add($"{statName} {flatText} / x{multiplierText}");
			}
		}

		return string.Join("\n", lines);
	}

	public static string GetFailureReasonText(EquipmentActionFailureReason reason, EquipmentSlotType slotType = EquipmentSlotType.None)
	{
		bool chinese = UiText.CurrentLocale == UiText.DefaultLocale;
		return reason switch
		{
			EquipmentActionFailureReason.MissingSlot => chinese ? "\u76ee\u6807\u69fd\u4f4d\u4e0d\u5b58\u5728" : "target slot is missing",
			EquipmentActionFailureReason.MissingEquipmentDefinition => chinese ? "\u8be5\u7269\u54c1\u4e0d\u53ef\u88c5\u5907" : "item is not equippable",
			EquipmentActionFailureReason.SlotNotAllowed => chinese
				? $"\u4e0d\u80fd\u88c5\u5907\u5230 {GetSlotDisplayName(slotType)}"
				: $"cannot equip to {GetSlotDisplayName(slotType)}",
			EquipmentActionFailureReason.CategoryNotAllowed => chinese
				? $"{GetSlotDisplayName(slotType)} \u4e0d\u63a5\u53d7\u8be5\u7c7b\u522b"
				: $"{GetSlotDisplayName(slotType)} rejects this category",
			EquipmentActionFailureReason.SlotOccupied => chinese
				? $"{GetSlotDisplayName(slotType)} \u5df2\u88ab\u5360\u7528"
				: $"{GetSlotDisplayName(slotType)} is occupied",
			EquipmentActionFailureReason.SecondaryWeaponOccupied => chinese ? "\u6b66\u5668\u69fd\u5df2\u88ab\u5360\u7528" : "weapon slot is occupied",
			EquipmentActionFailureReason.SlotEmpty => chinese
				? $"{GetSlotDisplayName(slotType)} \u5f53\u524d\u4e3a\u7a7a"
				: $"{GetSlotDisplayName(slotType)} is empty",
			EquipmentActionFailureReason.TargetContainerRejected => chinese ? "\u76ee\u6807\u80cc\u5305\u6ca1\u6709\u53ef\u7528\u4f4d\u7f6e" : "target inventory has no valid space",
			_ => chinese ? "\u6761\u4ef6\u4e0d\u6ee1\u8db3" : "requirements not met",
		};
	}

	private static string? FormatStatModifier(EquipmentDefinition equipment, EquipmentStatModifierDefinition modifier)
	{
		if (modifier.StatType == StatType.MaxHealth
			|| modifier.StatType == StatType.ArmorAbsorbRate
			|| modifier.StatType == StatType.ArmorPointMax)
		{
			return null;
		}

		if (IsWeapon(equipment) && modifier.StatType == StatType.AttackPower)
		{
			return null;
		}

		if (IsChestArmor(equipment) && modifier.StatType == StatType.MoveSpeed)
		{
			return null;
		}

		if (modifier.StatType == StatType.ArmorTier)
		{
			return null;
		}

		List<string> parts = [];
		string statName = GetStatDisplayName(modifier.StatType);

		if (!Mathf.IsZeroApprox(modifier.Flat))
		{
			string sign = modifier.Flat > 0.0f ? "+" : string.Empty;
			parts.Add($"{statName} {sign}{modifier.Flat:0.##}");
		}

		if (!Mathf.IsEqualApprox(modifier.Multiplier, 1.0f))
		{
			parts.Add($"{statName} x{modifier.Multiplier:0.##}");
		}

		return parts.Count == 0 ? $"{statName} +0" : string.Join(" / ", parts);
	}

	private static string FormatFireMode(FirearmFireMode fireMode, bool chinese)
	{
		return fireMode switch
		{
			FirearmFireMode.Automatic => chinese ? "全自动" : "Automatic",
			FirearmFireMode.Selective => chinese ? "单点+全自动" : "Selective",
			_ => chinese ? "单发" : "Single Shot"
		};
	}
}

