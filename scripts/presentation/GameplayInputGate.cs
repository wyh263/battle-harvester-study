using Godot;

namespace BattleHarvesterStudy.Presentation;

public partial class GameplayInputGate : Node
{
	private InventoryUiController? _inventoryUiController;
	private SettingsMenuController? _settingsMenuController;

	public GameplayInputBlockState CurrentState =>
		MergeStates(
			_inventoryUiController?.GetGameplayInputBlockState() ?? GameplayInputBlockState.None,
			_settingsMenuController?.GetGameplayInputBlockState() ?? GameplayInputBlockState.None);

	public bool IsBlocked => CurrentState.AnyBlocked;
	public bool BlocksMovementInput => CurrentState.MovementBlocked;
	public bool BlocksCombatInput => CurrentState.CombatBlocked;
	public bool BlocksTargetingInput => CurrentState.TargetingBlocked;
	public bool BlocksWorldInteraction => CurrentState.WorldInteractionBlocked;
	public bool BlocksCameraInput => CurrentState.CameraBlocked;

	public override void _Ready()
	{
		Node3D? owner = UiNodeLocator.ResolveGameplayRoot(this);
		_inventoryUiController = owner?.GetNodeOrNull<InventoryUiController>("GameUiRoot/Controllers/InventoryUiController");
		_settingsMenuController = owner?.GetNodeOrNull<SettingsMenuController>("GameUiRoot/Controllers/SettingsMenuController");
	}

	private static GameplayInputBlockState MergeStates(GameplayInputBlockState left, GameplayInputBlockState right)
	{
		return new GameplayInputBlockState(
			MovementBlocked: left.MovementBlocked || right.MovementBlocked,
			CombatBlocked: left.CombatBlocked || right.CombatBlocked,
			TargetingBlocked: left.TargetingBlocked || right.TargetingBlocked,
			WorldInteractionBlocked: left.WorldInteractionBlocked || right.WorldInteractionBlocked,
			CameraBlocked: left.CameraBlocked || right.CameraBlocked);
	}
}
