using BattleHarvesterStudy.Combat.Firearms;
using BattleHarvesterStudy.Items;

namespace BattleHarvesterStudy.Presentation;

public static class FirearmTextFormatter
{
	public static string GetAmmoTypeName(AmmoType ammoType, bool chinese)
	{
		return ammoType switch
		{
			AmmoType.ShotgunShell => chinese ? "\u6563\u5f39" : "Shotgun Shell",
			AmmoType.RifleRound => chinese ? "\u6b65\u67aa\u5f39" : "Rifle Round",
			AmmoType.SniperRound => chinese ? "\u72d9\u51fb\u5f39" : "Sniper Round",
			_ => chinese ? "\u65e0" : "None"
		};
	}

	public static string GetAmmoDisplayName(AmmoItemDefinition? ammo, bool chinese)
	{
		if (ammo == null)
		{
			return chinese ? "\u65e0\u5f39\u836f" : "No Ammo";
		}

		return chinese
			? $"T{ammo.AmmoTier} {GetAmmoTypeName(ammo.AmmoType, true)}"
			: $"T{ammo.AmmoTier} {GetAmmoTypeName(ammo.AmmoType, false)}";
	}

	public static string GetFireModeName(FirearmFireMode fireMode, bool chinese)
	{
		return fireMode switch
		{
			FirearmFireMode.Automatic => chinese ? "\u5168\u81ea\u52a8" : "Automatic",
			FirearmFireMode.Selective => chinese ? "\u5355\u70b9+\u5168\u81ea\u52a8" : "Selective",
			_ => chinese ? "\u5355\u53d1" : "Single Shot"
		};
	}
}
