using Godot;
using BattleHarvesterStudy.Inventory.Search;

namespace BattleHarvesterStudy.Items;

[GlobalClass]
public partial class UsableItemDefinition : Resource
{
	[Export]
	public SkillDefinition? CastSkill { get; set; }

	[Export(PropertyHint.Range, "0,99,1")]
	public int UsesPerItem { get; set; }

	[Export(PropertyHint.Range, "0,9999,0.1")]
	public float RestoreHealth { get; set; } = 0.0f;

	[Export(PropertyHint.Range, "0,9999,0.1")]
	public float RestoreResource { get; set; } = 0.0f;

	[Export]
	public bool RepairsArmor { get; set; }

	[Export(PropertyHint.Range, "1,20,1")]
	public int RepairTier { get; set; } = 1;

	[Export]
	public ContainerModifierProfileDefinition? ContainerModifierProfile { get; set; }

	[Export]
	public bool ConsumeOnUse { get; set; } = true;

	[Export]
	public bool AllowUseAtFull { get; set; }
}
