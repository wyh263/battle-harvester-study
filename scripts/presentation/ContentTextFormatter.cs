using System.Collections.Generic;
using BattleHarvesterStudy.Combat;
using BattleHarvesterStudy.Items;

namespace BattleHarvesterStudy.Presentation;

public static class ContentTextFormatter
{
	private static readonly Dictionary<string, string> ChineseItemNames = new()
	{
		["katana_t1"] = "T1 刀",
		["katana_t2"] = "T2 刀",
		["katana_t3"] = "T3 刀",
		["katana_t4"] = "T4 刀",
		["katana_t5"] = "T5 刀",
		["greatsword_t1"] = "T1 大剑",
		["greatsword_t2"] = "T2 大剑",
		["greatsword_t3"] = "T3 大剑",
		["greatsword_t4"] = "T4 大剑",
		["greatsword_t5"] = "T5 大剑",
		["dagger_t1"] = "T1 匕首",
		["dagger_t2"] = "T2 匕首",
		["dagger_t3"] = "T3 匕首",
		["dagger_t4"] = "T4 匕首",
		["dagger_t5"] = "T5 匕首",
		["shotgun_t3"] = "T3 散弹枪",
		["rifle_t3"] = "T3 步枪",
		["sniper_t3"] = "T3 狙击枪",
		["gloves_t1"] = "T1 手套",
		["gloves_t2"] = "T2 手套",
		["gloves_t3"] = "T3 手套",
		["gloves_t4"] = "T4 手套",
		["gloves_t5"] = "T5 手套",
		["armor_t1"] = "T1 护甲",
		["armor_t2"] = "T2 护甲",
		["armor_t3"] = "T3 护甲",
		["armor_t4"] = "T4 护甲",
		["armor_t5"] = "T5 护甲",
		["repair_kit_t1"] = "T1 甲修包",
		["repair_kit_t2"] = "T2 甲修包",
		["repair_kit_t3"] = "T3 甲修包",
		["repair_kit_t4"] = "T4 甲修包",
		["repair_kit_t5"] = "T5 甲修包",
		["weapon_skill_katana_draw_counter"] = "刀·居合反击手册",
		["weapon_skill_katana_phase_strike"] = "刀·瞬步斩手册",
		["weapon_skill_greatsword_armor_cleave"] = "大剑·破甲劈手册",
		["weapon_skill_greatsword_guard_crash"] = "大剑·崩防击手册",
		["weapon_skill_dagger_bleed_rush"] = "匕首·裂伤突袭手册",
		["weapon_skill_dagger_shadow_flurry"] = "匕首·影袭乱舞手册",
		["search_medical_kit"] = "搜索增幅器 I",
		["weapon_manifest_probe"] = "搜索增幅器 II",
		["tech_filter_chip"] = "爆率增幅器 I",
		["test_scrap"] = "测试废料",
		["test_core"] = "测试核心",
	};

	private static readonly Dictionary<string, string> ChineseSkillNames = new()
	{
		["basic_attack"] = "普通攻击",
		["basic_attack_followup"] = "追击",
		["basic_attack_finisher"] = "终结",
		["katana_basic_attack"] = "刀·起手斩",
		["katana_basic_attack_followup"] = "刀·追身斩",
		["katana_basic_attack_finisher"] = "刀·收势斩",
		["katana_draw_counter"] = "居合反击",
		["katana_phase_strike"] = "瞬步斩",
		["greatsword_basic_attack"] = "大剑·起手劈",
		["greatsword_basic_attack_followup"] = "大剑·横斩",
		["greatsword_basic_attack_finisher"] = "大剑·重压斩",
		["greatsword_armor_cleave"] = "破甲劈",
		["greatsword_guard_crash"] = "崩防击",
		["dagger_basic_attack"] = "匕首·刺击",
		["dagger_basic_attack_followup"] = "匕首·裂伤追刺",
		["dagger_basic_attack_finisher"] = "匕首·终结连刺",
		["dagger_bleed_rush"] = "裂伤突袭",
		["dagger_shadow_flurry"] = "影袭乱舞",
	};

	private static readonly Dictionary<string, string> ChineseMoveSetNames = new()
	{
		["moveset_katana"] = "刀攻击组",
		["moveset_greatsword"] = "大剑攻击组",
		["moveset_dagger"] = "匕首攻击组",
	};

	private static readonly Dictionary<string, string> ChineseTags = new()
	{
		["equipment"] = "装备",
		["weapon"] = "武器",
		["armor"] = "护甲",
		["gloves"] = "手套",
		["sharp"] = "锐器",
		["blunt"] = "钝器",
		["katana"] = "刀",
		["greatsword"] = "大剑",
		["dagger"] = "匕首",
		["firearm"] = "枪械",
		["shotgun"] = "散弹枪",
		["rifle"] = "步枪",
		["sniper"] = "狙击枪",
		["manual"] = "手册",
		["weapon_skill"] = "武器技能",
		["counter"] = "反击",
		["bleed"] = "流血",
		["tier1"] = "T1",
		["tier2"] = "T2",
		["tier3"] = "T3",
		["tier4"] = "T4",
		["tier5"] = "T5",
	};

	public static string GetItemDisplayName(ItemDefinition definition)
	{
		if (definition.Ammo != null)
		{
			bool chinese = UiText.CurrentLocale == UiText.DefaultLocale;
			return FirearmTextFormatter.GetAmmoDisplayName(definition.Ammo, chinese);
		}

		if (UiText.CurrentLocale != UiText.DefaultLocale)
		{
			return definition.DisplayName;
		}

		return ChineseItemNames.TryGetValue(definition.ItemId, out string? localizedName)
			? localizedName
			: definition.DisplayName;
	}

	public static string GetSkillDisplayName(SkillDefinition? skill)
	{
		if (skill == null)
		{
			return "-";
		}

		if (UiText.CurrentLocale != UiText.DefaultLocale)
		{
			return skill.DisplayName;
		}

		return ChineseSkillNames.TryGetValue(skill.SkillId, out string? localizedName)
			? localizedName
			: skill.DisplayName;
	}

	public static string GetMoveSetDisplayName(WeaponMoveSetDefinition? moveSet)
	{
		if (moveSet == null)
		{
			return "-";
		}

		if (UiText.CurrentLocale != UiText.DefaultLocale)
		{
			return moveSet.DisplayName;
		}

		return ChineseMoveSetNames.TryGetValue(moveSet.MoveSetId, out string? localizedName)
			? localizedName
			: moveSet.DisplayName;
	}

	public static string FormatTags(IEnumerable<string> tags)
	{
		List<string> values = [];
		foreach (string tag in tags)
		{
			values.Add(GetTagDisplayName(tag));
		}

		return values.Count == 0 ? "-" : string.Join(", ", values);
	}

	public static string GetTagDisplayName(string tag)
	{
		if (UiText.CurrentLocale != UiText.DefaultLocale)
		{
			return tag;
		}

		return ChineseTags.TryGetValue(tag, out string? localizedName)
			? localizedName
			: tag;
	}
}
