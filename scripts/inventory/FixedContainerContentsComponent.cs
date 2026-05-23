using Godot;
using Godot.Collections;
using BattleHarvesterStudy.Items;

namespace BattleHarvesterStudy.Inventory;

public partial class FixedContainerContentsComponent : Node
{
	[Export]
	public NodePath ContainerPath { get; set; } = "../GridContainer";

	[Export]
	public Array<ItemDefinition> Items { get; set; } = [];

	[Export]
	public Array<int> StackCounts { get; set; } = [];

	[Export]
	public bool FillOnReady { get; set; } = true;

	[Export]
	public bool ClearContainerBeforeFill { get; set; } = true;

	[Export]
	public bool FillOnlyWhenEmpty { get; set; }

	private GridContainerComponent? _container;

	public override void _Ready()
	{
		_container = GetNodeOrNull<GridContainerComponent>(ContainerPath);
		if (FillOnReady)
		{
			CallDeferred(MethodName.Fill);
		}
	}

	public bool Fill()
	{
		if (_container == null)
		{
			return false;
		}

		if (FillOnlyWhenEmpty && _container.ItemRecords.Count > 0)
		{
			return false;
		}

		if (ClearContainerBeforeFill)
		{
			_container.Clear();
		}

		bool addedAny = false;
		for (int index = 0; index < Items.Count; index++)
		{
			ItemDefinition definition = Items[index];
			if (definition == null)
			{
				continue;
			}

			int stackCount = index < StackCounts.Count ? Mathf.Max(1, StackCounts[index]) : 1;
			ItemInstance item = new(definition, stackCount);
			item.SetAcquisitionState(ItemAcquisitionState.Base);
			if (_container.TryAcceptItem(item))
			{
				addedAny = true;
			}
			else
			{
				GD.PushWarning($"Failed to place test item {definition.ItemId} into container {_container.Name}.");
			}
		}

		return addedAny;
	}
}
