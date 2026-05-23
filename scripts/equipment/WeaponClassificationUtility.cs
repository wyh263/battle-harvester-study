using System.Collections.Generic;

namespace BattleHarvesterStudy.Equipment;

public static class WeaponClassificationUtility
{
	public static bool MatchesAny(WeaponArchetype value, int requiredMask)
	{
		return requiredMask == 0 || (((int)value) & requiredMask) != 0;
	}

	public static bool MatchesAny(WeaponFamily value, int requiredMask)
	{
		return requiredMask == 0 || (((int)value) & requiredMask) != 0;
	}

	public static bool ContainsAll(int ownedMask, int requiredMask)
	{
		return requiredMask == 0 || (ownedMask & requiredMask) == requiredMask;
	}

	public static string FormatTraits(int traitMask, bool chinese)
	{
		if (traitMask == 0)
		{
			return "-";
		}

		List<string> names = [];
		foreach (WeaponTrait trait in System.Enum.GetValues(typeof(WeaponTrait)))
		{
			if (trait == WeaponTrait.None || (traitMask & (int)trait) == 0)
			{
				continue;
			}

			names.Add(GetTraitName(trait, chinese));
		}

		return names.Count == 0 ? "-" : string.Join(", ", names);
	}

	public static string FormatFamilies(int familyMask, bool chinese)
	{
		if (familyMask == 0)
		{
			return "-";
		}

		List<string> names = [];
		foreach (WeaponFamily family in System.Enum.GetValues(typeof(WeaponFamily)))
		{
			if (family == WeaponFamily.None || (familyMask & (int)family) == 0)
			{
				continue;
			}

			names.Add(GetFamilyName(family, chinese));
		}

		return names.Count == 0 ? "-" : string.Join(", ", names);
	}

	public static string FormatArchetypes(int archetypeMask, bool chinese)
	{
		if (archetypeMask == 0)
		{
			return "-";
		}

		List<string> names = [];
		foreach (WeaponArchetype archetype in System.Enum.GetValues(typeof(WeaponArchetype)))
		{
			if (archetype == WeaponArchetype.None || (archetypeMask & (int)archetype) == 0)
			{
				continue;
			}

			names.Add(GetArchetypeName(archetype, chinese));
		}

		return names.Count == 0 ? "-" : string.Join(", ", names);
	}

	public static string GetArchetypeName(WeaponArchetype archetype, bool chinese)
	{
		return archetype switch
		{
			WeaponArchetype.Sharp => chinese ? "锐器" : "Sharp",
			WeaponArchetype.Blunt => chinese ? "钝器" : "Blunt",
			WeaponArchetype.Projectile => chinese ? "远程/投射" : "Projectile",
			WeaponArchetype.Flexible => chinese ? "奇兵/链系" : "Flexible",
			WeaponArchetype.Mechanism => chinese ? "构装/重器" : "Mechanism",
			_ => "-"
		};
	}

	public static string GetFamilyName(WeaponFamily family, bool chinese)
	{
		return family switch
		{
			WeaponFamily.Sword => chinese ? "剑" : "Sword",
			WeaponFamily.Katana => chinese ? "刀" : "Katana",
			WeaponFamily.Greatsword => chinese ? "大剑" : "Greatsword",
			WeaponFamily.Dagger => chinese ? "匕首" : "Dagger",
			WeaponFamily.Spear => chinese ? "枪" : "Spear",
			WeaponFamily.Hammer => chinese ? "锤" : "Hammer",
			WeaponFamily.Axe => chinese ? "斧" : "Axe",
			WeaponFamily.Mace => chinese ? "锏" : "Mace",
			WeaponFamily.ThrowingKnife => chinese ? "飞刀" : "Throwing Knife",
			WeaponFamily.BoltLauncher => chinese ? "弩箭" : "Bolt Launcher",
			WeaponFamily.Whip => chinese ? "鞭" : "Whip",
			WeaponFamily.ChainBlade => chinese ? "链刃" : "Chain Blade",
			WeaponFamily.Drill => chinese ? "钻头" : "Drill",
			WeaponFamily.Flamethrower => chinese ? "喷火器" : "Flamethrower",
			WeaponFamily.Shotgun => chinese ? "散弹枪" : "Shotgun",
			WeaponFamily.Rifle => chinese ? "步枪" : "Rifle",
			WeaponFamily.SniperRifle => chinese ? "狙击枪" : "Sniper Rifle",
			_ => "-"
		};
	}

	public static string GetTraitName(WeaponTrait trait, bool chinese)
	{
		return trait switch
		{
			WeaponTrait.RecoveryPunish => chinese ? "破绽追击" : "Recovery Punish",
			WeaponTrait.HeavySwing => chinese ? "重挥" : "Heavy Swing",
			WeaponTrait.Bleed => chinese ? "流血" : "Bleed",
			WeaponTrait.Execution => chinese ? "处决" : "Execution",
			WeaponTrait.MultiHit => chinese ? "多段" : "Multi Hit",
			WeaponTrait.Sustained => chinese ? "持续" : "Sustained",
			WeaponTrait.ArmorBreak => chinese ? "破甲" : "Armor Break",
			WeaponTrait.Reach => chinese ? "长柄" : "Reach",
			WeaponTrait.Bypass => chinese ? "绕过" : "Bypass",
			_ => "-"
		};
	}
}
