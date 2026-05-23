using Godot;

namespace BattleHarvesterStudy.Items;

[GlobalClass]
public partial class AmmoItemDefinition : Resource
{
	[Export] public AmmoType AmmoType { get; set; } = AmmoType.None;
	[Export(PropertyHint.Range, "1,5,1")] public int AmmoTier { get; set; } = 1;
	[Export(PropertyHint.Range, "0,10,0.1")] public float PenetrationTier { get; set; } = 1.0f;

	public static string BuildDefaultAmmoItemId(AmmoType ammoType, int tier)
	{
		string prefix = ammoType switch
		{
			AmmoType.ShotgunShell => "shotgun_ammo",
			AmmoType.RifleRound => "rifle_ammo",
			AmmoType.SniperRound => "sniper_ammo",
			_ => "ammo_generic"
		};

		return $"{prefix}_t{Mathf.Max(1, tier)}";
	}
}
