using Godot;
using Godot.Collections;
using BattleHarvesterStudy.Inventory.Search;

namespace BattleHarvesterStudy.Presentation;

public partial class InventoryUiController : Node
{
	[Signal]
	public delegate void UiStateChangedEventHandler();

	[Signal]
	public delegate void StatusTextChangedEventHandler(string textKey, Dictionary<string, Variant> formatArgs);

	private const string DefaultInventoryToggleAction = "toggle_inventory";
	private const string DefaultCloseContainerAction = "lock_exit";

	[Export]
	public NodePath InteractionControllerPath { get; set; } = new("../InventoryInteractionController");

	[Export]
	public string InventoryToggleAction { get; set; } = DefaultInventoryToggleAction;

	[Export]
	public string CloseContainerAction { get; set; } = DefaultCloseContainerAction;

	public bool PlayerWindowOpen { get; private set; }
	public WorldContainer? ActiveContainer { get; private set; }
	public GridContainerComponent? ActiveExternalContainer { get; private set; }
	public string ActiveExternalContainerName { get; private set; } = string.Empty;
	public bool ActiveExternalContainerIsWarehouse => ActiveContainer?.IsWarehouseContainer() ?? false;
	public bool BlocksGameplayInput => PlayerWindowOpen || ActiveExternalContainer != null;
	private InventoryInteractionController? _interactionController;
	private ulong _suppressCloseContainerUntilFrame;

	public GameplayInputBlockState GetGameplayInputBlockState()
	{
		bool blocked = BlocksGameplayInput;
		return new GameplayInputBlockState(
			MovementBlocked: blocked,
			CombatBlocked: blocked,
			TargetingBlocked: blocked,
			WorldInteractionBlocked: blocked,
			CameraBlocked: false);
	}

	public override void _Ready()
	{
		_interactionController = GetNodeOrNull<InventoryInteractionController>(InteractionControllerPath);
		ContainerInteractionEvents.InteractionPublished += OnInteractionPublished;
		SetProcess(true);
	}

	public override void _ExitTree()
	{
		ContainerInteractionEvents.InteractionPublished -= OnInteractionPublished;
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed(InventoryToggleAction))
		{
			TogglePlayerInventoryWindow();
		}

		if (Input.IsActionJustPressed(CloseContainerAction)
			&& Engine.GetProcessFrames() > _suppressCloseContainerUntilFrame)
		{
			CloseContainerWindow();
		}

		if (ActiveContainer != null && !GodotObject.IsInstanceValid(ActiveContainer))
		{
			ActiveContainer = null;
			ActiveExternalContainer = null;
			ActiveExternalContainerName = string.Empty;
			EmitSignal(SignalName.UiStateChanged);
		}
	}

	public bool TryHandleCancelAction()
	{
		if (_interactionController?.TryCloseTopDetailTarget() == true)
		{
			return true;
		}

		if (ActiveExternalContainer != null)
		{
			CloseContainerWindow();
			return true;
		}

		if (PlayerWindowOpen)
		{
			ClosePlayerInventoryWindow();
			return true;
		}

		return false;
	}

	private void OnInteractionPublished(ContainerInteractionEvent interactionEvent)
	{
		SetContainerSearchActive(ActiveContainer, false);

		if (!interactionEvent.Succeeded)
		{
			EmitSignal(
				SignalName.StatusTextChanged,
				string.IsNullOrWhiteSpace(interactionEvent.AccessResult.FailureTextKey)
					? UiTextKeys.Inventory.StatusCannotAccess
					: interactionEvent.AccessResult.FailureTextKey,
				interactionEvent.AccessResult.FailureFormatArgs);
			return;
		}

		PlayerWindowOpen = true;
		ActiveContainer = interactionEvent.Container;
		ActiveExternalContainer = interactionEvent.Container.GetGridContainer();
		ActiveExternalContainerName = interactionEvent.Container.GetDisplayName();
		_suppressCloseContainerUntilFrame = Engine.GetProcessFrames() + 1;
		SetContainerSearchActive(ActiveContainer, true);
		EmitSignal(
			SignalName.StatusTextChanged,
			UiTextKeys.Inventory.StatusOpenedContainer,
			UiTextArgs.Create(("name", interactionEvent.Container.GetDisplayName())));
		EmitSignal(SignalName.UiStateChanged);
	}

	private void TogglePlayerInventoryWindow()
	{
		PlayerWindowOpen = !PlayerWindowOpen;
		if (!PlayerWindowOpen)
		{
			SetContainerSearchActive(ActiveContainer, false);
			ActiveContainer = null;
			ActiveExternalContainer = null;
			ActiveExternalContainerName = string.Empty;
			EmitSignal(
				SignalName.StatusTextChanged,
				UiTextKeys.Inventory.StatusClosedInventory,
				UiTextArgs.Create());
		}
		else
		{
			EmitSignal(
				SignalName.StatusTextChanged,
				UiTextKeys.Inventory.StatusOpenedInventory,
				UiTextArgs.Create());
		}

		EmitSignal(SignalName.UiStateChanged);
	}

	public void ClosePlayerInventoryWindow()
	{
		if (!PlayerWindowOpen && ActiveExternalContainer == null)
		{
			return;
		}

		SetContainerSearchActive(ActiveContainer, false);
		PlayerWindowOpen = false;
		ActiveContainer = null;
		ActiveExternalContainer = null;
		ActiveExternalContainerName = string.Empty;
		EmitSignal(
			SignalName.StatusTextChanged,
			UiTextKeys.Inventory.StatusClosedInventory,
			UiTextArgs.Create());
		EmitSignal(SignalName.UiStateChanged);
	}

	public void CloseContainerWindow()
	{
		if (ActiveExternalContainer == null)
		{
			return;
		}

		SetContainerSearchActive(ActiveContainer, false);
		EmitSignal(
			SignalName.StatusTextChanged,
			UiTextKeys.Inventory.StatusClosedContainer,
			UiTextArgs.Create(("name", string.IsNullOrWhiteSpace(ActiveExternalContainerName) ? UiText.Resolve(UiTextKeys.Inventory.HeaderContainer) : ActiveExternalContainerName)));
		ActiveContainer = null;
		ActiveExternalContainer = null;
		ActiveExternalContainerName = string.Empty;
		EmitSignal(SignalName.UiStateChanged);
	}

	public void PublishStatus(string textKey, Dictionary<string, Variant> formatArgs)
	{
		EmitSignal(SignalName.StatusTextChanged, textKey, formatArgs);
	}

	private static void SetContainerSearchActive(WorldContainer? container, bool active)
	{
		container?.GetSearchRuntime()?.SetSearchActive(active);
	}
}
