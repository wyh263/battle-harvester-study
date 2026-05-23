using Godot;
using Godot.Collections;

namespace BattleHarvesterStudy.Inventory;

[GlobalClass]
public partial class WorldContainerArchetypeDefinition : Resource
{
	[Export]
	public string ArchetypeId { get; set; } = "world_container_generic";

	[Export]
	public string DisplayName { get; set; } = "Container";

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

	[Export]
	public LootTableDefinition? LootTable { get; set; }

	[Export]
	public Vector3 VisualSize { get; set; } = new(1.2f, 0.8f, 0.8f);

	[Export]
	public Color VisualColor { get; set; } = new(0.56f, 0.40f, 0.23f, 1.0f);

	public GridContainerDefinition BuildGridDefinition()
	{
		return new GridContainerDefinition
		{
			ContainerId = ArchetypeId,
			DisplayName = DisplayName,
			VisualType = VisualType,
			Tags = [.. Tags],
			GridColumns = GridColumns,
			GridRows = GridRows,
			AllowedCategories = [.. AllowedCategories],
			AutoSortEnabled = AutoSortEnabled,
			RequiresSearch = RequiresSearch
		};
	}
}
