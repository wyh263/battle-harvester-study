using Godot;
using System.Text;
using BattleHarvesterStudy.Presentation;
using BattleHarvesterStudy.Inventory.Search;

namespace BattleHarvesterStudy.Inventory;

public partial class WorldContainer : Node3D
{
	private const string DefaultInteractAction = "interact";

	[Export]
	public NodePath RequesterPath { get; set; } = "../Player";

	[Export]
	public WorldContainerArchetypeDefinition? Archetype { get; set; }

	[Export]
	public NodePath GridContainerPath { get; set; } = "Components/GridContainer";

	[Export]
	public NodePath AccessControllerPath { get; set; } = "Components/AccessController";

	[Export]
	public NodePath LootComponentPath { get; set; } = "Components/ContainerLoot";

	[Export]
	public NodePath StatusLabelPath { get; set; } = "Visuals/StatusLabel";

	[Export]
	public NodePath ChestMeshPath { get; set; } = "Visuals/ChestMesh";

	[Export]
	public NodePath SearchRuntimePath { get; set; } = "Components/ContainerSearchRuntime";

	[Export]
	public string InteractAction { get; set; } = DefaultInteractAction;

	[Export]
	public bool PrintContentsOnInteract { get; set; } = true;

	private GridContainerComponent? _gridContainer;
	private ContainerAccessController? _accessController;
	private ContainerLootComponent? _lootComponent;
	private ContainerSearchRuntimeComponent? _searchRuntime;
	private Label3D? _statusLabel;
	private MeshInstance3D? _chestMesh;

	public override void _EnterTree()
	{
		ResolveNodes();
		ApplyArchetype();
	}

	public override void _Ready()
	{
		ResolveNodes();
		ApplyArchetype();

		if (_gridContainer != null)
		{
			_gridContainer.ContainerChanged += RefreshStatusLabel;
		}

		UiText.LanguageChanged += RefreshStatusLabel;

		RefreshStatusLabel();
		SetPhysicsProcess(true);
	}

	public override void _ExitTree()
	{
		if (_gridContainer != null)
		{
			_gridContainer.ContainerChanged -= RefreshStatusLabel;
		}

		UiText.LanguageChanged -= RefreshStatusLabel;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!Input.IsActionJustPressed(InteractAction))
		{
			return;
		}

		Node3D? requester = GetNodeOrNull<Node3D>(RequesterPath);
		if (!CanAcceptInteractionInput(requester))
		{
			return;
		}

		TryInteract(requester);
	}

	public bool TryInteract(Node3D? requester)
	{
		if (_accessController == null)
		{
			return false;
		}

		if (!_accessController.TryConsumeAccess(requester, out ContainerAccessCheckResult result))
		{
			GD.Print($"[Container] {Name}: {UiText.Resolve(result.FailureTextKey, result.FailureFormatArgs)}");
			ContainerInteractionEvents.Publish(new ContainerInteractionEvent
			{
				Container = this,
				Succeeded = false,
				AccessResult = result
			});
			RefreshStatusLabel();
			return false;
		}

		_searchRuntime?.SetSearchRequester(requester);
		_lootComponent?.TryFill(requester);
		ContainerInteractionEvents.Publish(new ContainerInteractionEvent
		{
			Container = this,
			Succeeded = true,
			AccessResult = result
		});

		if (PrintContentsOnInteract)
		{
			GD.Print(BuildContentsSummary());
		}

		RefreshStatusLabel();
		return true;
	}

	public string GetDisplayName()
	{
		_gridContainer ??= GetNodeOrNull<GridContainerComponent>(GridContainerPath);
		return _gridContainer?.Definition?.DisplayName ?? Name;
	}

	public GridContainerComponent? GetGridContainer()
	{
		_gridContainer ??= GetNodeOrNull<GridContainerComponent>(GridContainerPath);
		return _gridContainer;
	}

	public bool IsWarehouseContainer()
	{
		GridContainerDefinition? definition = _gridContainer?.Definition;
		if (definition == null)
		{
			return false;
		}

		return string.Equals(definition.ContainerId, "home_warehouse", System.StringComparison.OrdinalIgnoreCase)
			|| string.Equals(definition.DisplayName, "Warehouse", System.StringComparison.OrdinalIgnoreCase);
	}

	public ContainerAccessController? GetAccessController()
	{
		_accessController ??= GetNodeOrNull<ContainerAccessController>(AccessControllerPath);
		return _accessController;
	}

	public ContainerSearchRuntimeComponent? GetSearchRuntime()
	{
		_searchRuntime ??= GetNodeOrNull<ContainerSearchRuntimeComponent>(SearchRuntimePath);
		return _searchRuntime;
	}

	private void ResolveNodes()
	{
		_gridContainer ??= GetNodeOrNull<GridContainerComponent>(GridContainerPath);
		_accessController ??= GetNodeOrNull<ContainerAccessController>(AccessControllerPath);
		_lootComponent ??= GetNodeOrNull<ContainerLootComponent>(LootComponentPath);
		_searchRuntime ??= GetNodeOrNull<ContainerSearchRuntimeComponent>(SearchRuntimePath);
		_statusLabel ??= GetNodeOrNull<Label3D>(StatusLabelPath);
		_chestMesh ??= GetNodeOrNull<MeshInstance3D>(ChestMeshPath);
	}

	private void ApplyArchetype()
	{
		if (Archetype == null)
		{
			return;
		}

		if (_gridContainer != null)
		{
			_gridContainer.Definition = Archetype.BuildGridDefinition();
		}

		if (_lootComponent != null)
		{
			_lootComponent.LootTable = Archetype.LootTable;
		}

		if (_statusLabel != null)
		{
			_statusLabel.Text = Archetype.DisplayName;
		}

		if (_chestMesh != null)
		{
			if (_chestMesh.Mesh is BoxMesh boxMesh)
			{
				BoxMesh meshCopy = (BoxMesh)boxMesh.Duplicate();
				meshCopy.Size = Archetype.VisualSize;
				_chestMesh.Mesh = meshCopy;
			}

			if (_chestMesh.GetActiveMaterial(0) is StandardMaterial3D material)
			{
				StandardMaterial3D materialCopy = (StandardMaterial3D)material.Duplicate();
				materialCopy.AlbedoColor = Archetype.VisualColor;
				_chestMesh.SetSurfaceOverrideMaterial(0, materialCopy);
			}
		}
	}

	private string BuildContentsSummary()
	{
		if (_gridContainer == null)
		{
			return $"[Container] {Name}: missing grid container";
		}

		if (_gridContainer.ItemRecords.Count == 0)
		{
			return $"[Container] {Name}: empty";
		}

		StringBuilder builder = new();
		builder.Append($"[Container] {Name}: ");
		bool first = true;
		foreach (ContainerItemRecord record in _gridContainer.ItemRecords)
		{
			if (!first)
			{
				builder.Append(", ");
			}

			bool revealed = _searchRuntime?.CanInteractWithItem(record.Item.InstanceId) != false;
			builder.Append(revealed
				? record.Item.Definition.DisplayName
				: (UiText.CurrentLocale == UiText.DefaultLocale ? "未搜索物品" : "Unsearched Item"));
			builder.Append(" x");
			builder.Append(record.Item.StackCount);
			first = false;
		}

		return builder.ToString();
	}

	private static bool CanAcceptInteractionInput(Node3D? requester)
	{
		if (requester == null)
		{
			return true;
		}

		GameplayInputGate? gameplayInputGate = requester.GetNodeOrNull<GameplayInputGate>("Components/GameplayInputGate");
		return !(gameplayInputGate?.BlocksWorldInteraction ?? false);
	}

	private void RefreshStatusLabel()
	{
		if (_statusLabel == null)
		{
			return;
		}

		string accessText = _accessController == null
			? UiText.Resolve(UiTextKeys.World.ContainerUnavailable)
			: _accessController.IsLocked
				? UiText.Resolve(UiTextKeys.World.ContainerLocked)
				: _accessController.SingleUseAccess && _accessController.HasBeenAccessed
					? UiText.Resolve(UiTextKeys.World.ContainerConsumed)
					: UiText.Resolve(UiTextKeys.World.ContainerOpenable);

		int itemCount = _gridContainer?.ItemRecords.Count ?? 0;
		_statusLabel.Text = UiText.Resolve(
			UiTextKeys.World.ContainerLabel,
			("access", accessText),
			("item_count", itemCount));
	}
}
