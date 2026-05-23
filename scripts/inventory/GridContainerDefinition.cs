using Godot;
using Godot.Collections;

namespace BattleHarvesterStudy.Inventory;

[GlobalClass]
public partial class GridContainerDefinition : Resource
{
	[Export]
	public string ContainerId { get; set; } = "container_generic";

	[Export]
	public string DisplayName { get; set; } = "Generic Container";

	[Export]
	public ContainerVisualType VisualType { get; set; } = ContainerVisualType.Generic;

	[Export]
	public Array<ContainerTag> Tags { get; set; } = [];

	[Export(PropertyHint.Range, "1,100,1")]
	public int GridColumns { get; set; } = 6;

	[Export(PropertyHint.Range, "1,100,1")]
	public int GridRows { get; set; } = 6;

	[Export]
	public Array<ItemCategory> AllowedCategories { get; set; } = [];

	[Export]
	public bool AutoSortEnabled { get; set; } = true;

	[Export]
	public bool RequiresSearch { get; set; } = true;

	[Export(PropertyHint.Range, "-1,1,0.001")]
	public float SecureBaseGreedRateOverride { get; set; } = -1.0f;

	public bool AcceptsCategory(ItemCategory category)
	{
		return AllowedCategories.Count == 0 || AllowedCategories.Contains(category);
	}

	public bool HasTag(ContainerTag tag)
	{
		return tag != ContainerTag.None && Tags.Contains(tag);
	}
}
