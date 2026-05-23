using Godot;
using System.Collections.Generic;

namespace BattleHarvesterStudy.Inventory;

public partial class ContainerLootComponent : Node
{
	[Signal]
	public delegate void LootFilledEventHandler(int addedItemCount, int rejectedItemCount);

	[Export]
	public LootTableDefinition? LootTable { get; set; }

	[Export]
	public NodePath ContainerPath { get; set; } = "../GridContainer";

	[Export]
	public bool FillOnReady { get; set; } = true;

	[Export]
	public bool FillOnlyOnce { get; set; } = true;

	[Export]
	public bool ClearContainerBeforeFill { get; set; }

	public bool HasFilled { get; private set; }

	private GridContainerComponent? _container;

	public override void _Ready()
	{
		_container = ResolveContainer();
		if (FillOnReady)
		{
			TryFill();
		}
	}

	public bool TryFill(Node3D? requester = null, RandomNumberGenerator? random = null)
	{
		if (LootTable == null || _container == null)
		{
			return false;
		}

		if (FillOnlyOnce && HasFilled)
		{
			return false;
		}

		if (ClearContainerBeforeFill)
		{
			_container.Clear();
		}

		List<ItemInstance> generatedItems = LootGenerator.GenerateItems(LootTable, random, new LootGenerationContext
		{
			Requester = requester,
			Container = GetParentOrNull<WorldContainer>() ?? GetOwnerOrNull<WorldContainer>()
		});
		int addedItemCount = 0;
		int rejectedItemCount = 0;
		foreach (ItemInstance item in generatedItems)
		{
			if (_container.TryAcceptItem(item))
			{
				addedItemCount++;
			}
			else
			{
				rejectedItemCount++;
			}
		}

		HasFilled = true;
		EmitSignal(SignalName.LootFilled, addedItemCount, rejectedItemCount);
		return addedItemCount > 0;
	}

	private GridContainerComponent? ResolveContainer()
	{
		if (ContainerPath.IsEmpty)
		{
			return GetParent()?.GetNodeOrNull<GridContainerComponent>("GridContainer");
		}

		return GetNodeOrNull<GridContainerComponent>(ContainerPath);
	}
}
