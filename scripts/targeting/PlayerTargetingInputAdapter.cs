using Godot;
using Godot.Collections;
using BattleHarvesterStudy.Presentation;

namespace BattleHarvesterStudy.Targeting;

public partial class PlayerTargetingInputAdapter : Node
{
	private const string LockEnterAction = "lock_enter";
	private const string LockExitAction = "lock_exit";
	private const string LockModeToggleAction = "lock_mode_toggle";

	private Node3D? _owner;
	private CombatAimController? _aimController;
	private PlayerTargetingPreferences? _preferences;
	private GameplayInputGate? _gameplayInputGate;

	public override void _Ready()
	{
		_owner = GetOwner<Node3D>();
		_aimController = _owner?.GetNodeOrNull<CombatAimController>("Components/CombatAimController");
		_preferences = _owner?.GetNodeOrNull<PlayerTargetingPreferences>("Components/PlayerTargetingPreferences");
		_gameplayInputGate = _owner?.GetNodeOrNull<GameplayInputGate>("Components/GameplayInputGate");
		SetPhysicsProcess(true);
		SetProcessUnhandledInput(true);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_aimController == null)
		{
			return;
		}

		if (_gameplayInputGate?.BlocksTargetingInput ?? false)
		{
			return;
		}

		if (GetDefaultLockMode() == LockAcquisitionMode.MouseFollow && _aimController.CurrentState == TargetingState.Locked)
		{
			UpdateMouseFollowAim();
		}

		if (Input.IsActionJustPressed(LockEnterAction)
			&& _aimController.CurrentState == TargetingState.Free
			&& GetDefaultLockMode() == LockAcquisitionMode.StrategyBased)
		{
			RequestLock();
			return;
		}

		if (Input.IsActionJustPressed(LockEnterAction)
			&& _aimController.CurrentState == TargetingState.Free
			&& GetDefaultLockMode() == LockAcquisitionMode.MouseFollow)
		{
			RequestMouseFollowLock();
			return;
		}

		if (Input.IsActionJustPressed(LockExitAction) && _aimController.CurrentState == TargetingState.Locked)
		{
			RequestUnlock();
			return;
		}

		if (Input.IsActionJustPressed(LockModeToggleAction))
		{
			ToggleDefaultLockMode();
		}
	}

	public void RequestLock()
	{
		if (_aimController == null)
		{
			return;
		}

		if (_gameplayInputGate?.BlocksTargetingInput ?? false)
		{
			return;
		}

		TargetSelectionStrategyKind strategy = _preferences?.DefaultLockStrategy ?? TargetSelectionStrategyKind.Nearest;
		LockTargetProviderRegistry
			.Get(LockAcquisitionMode.StrategyBased)
			.TryLock(_aimController, LockRequest.ForStrategy(strategy));
	}

	public void RequestUnlock()
	{
		_aimController?.EnterFreeState();
	}

	public void RequestMouseFollowLock()
	{
		if (_aimController == null)
		{
			return;
		}

		LockTargetProviderRegistry
			.Get(LockAcquisitionMode.MouseFollow)
			.TryLock(_aimController, LockRequest.ForStrategy(TargetSelectionStrategyKind.Nearest));
		UpdateMouseFollowAim();
	}

	public void RequestCycleNext()
	{
		_aimController?.CycleTarget(1);
	}

	public void SelectDefaultLockStrategy(TargetSelectionStrategyKind strategy)
	{
		if (_preferences == null)
		{
			return;
		}

		_preferences.DefaultLockStrategy = strategy;
	}

	public void SelectDefaultLockMode(LockAcquisitionMode mode)
	{
		if (_preferences == null)
		{
			return;
		}

		_preferences.DefaultLockMode = mode;
	}

	public void ToggleDefaultLockMode()
	{
		LockAcquisitionMode nextMode = GetDefaultLockMode() switch
		{
			LockAcquisitionMode.StrategyBased => LockAcquisitionMode.MouseDoubleClick,
			LockAcquisitionMode.MouseDoubleClick => LockAcquisitionMode.MouseFollow,
			_ => LockAcquisitionMode.StrategyBased
		};

		SelectDefaultLockMode(nextMode);
		GD.Print($"[Targeting] Default lock mode -> {nextMode}");
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (_aimController == null)
		{
			return;
		}

		if (@event is not InputEventMouseButton mouseButton)
		{
			return;
		}

		if (!mouseButton.Pressed || mouseButton.ButtonIndex != MouseButton.Left || !mouseButton.DoubleClick)
		{
			return;
		}

		if (GetDefaultLockMode() != LockAcquisitionMode.MouseDoubleClick)
		{
			return;
		}

		if (TryGetClickedTarget(mouseButton.Position, out Targetable? target))
		{
			LockTargetProviderRegistry
				.Get(LockAcquisitionMode.MouseDoubleClick)
				.TryLock(_aimController, LockRequest.ForExplicitTarget(LockAcquisitionMode.MouseDoubleClick, target));
		}
	}

	private LockAcquisitionMode GetDefaultLockMode()
	{
		return _preferences?.DefaultLockMode ?? LockAcquisitionMode.StrategyBased;
	}

	private void UpdateMouseFollowAim()
	{
		if (_aimController == null)
		{
			return;
		}

		if (TryGetMouseAimDirection(out Vector3 aimDirection))
		{
			_aimController.SetAimOverrideDirection(aimDirection);
		}
	}

	private bool TryGetClickedTarget(Vector2 screenPosition, out Targetable? target)
	{
		target = null;
		Viewport? viewport = GetViewport();
		Camera3D? camera = viewport?.GetCamera3D();
		if (viewport == null || camera == null)
		{
			return false;
		}

		Vector3 rayOrigin = camera.ProjectRayOrigin(screenPosition);
		Vector3 rayDirection = camera.ProjectRayNormal(screenPosition);
		PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(
			rayOrigin,
			rayOrigin + rayDirection * 200.0f);
		query.CollideWithAreas = true;
		query.CollideWithBodies = true;

		Dictionary result = viewport.World3D.DirectSpaceState.IntersectRay(query);
		if (result.Count == 0 || !result.ContainsKey("collider"))
		{
			return false;
		}

		Variant colliderValue = result["collider"];
		GodotObject? colliderObject = colliderValue.AsGodotObject();
		if (colliderObject is not Node collider)
		{
			return false;
		}

		target = ResolveTargetable(collider);
		return target != null;
	}

	private static Targetable? ResolveTargetable(Node collider)
	{
		Node? current = collider;
		while (current != null)
		{
			if (current is Targetable targetable && targetable.CanBeTargeted)
			{
				return targetable;
			}

			Targetable? nested = current.GetNodeOrNull<Targetable>("Components/Targetable");
			if (nested != null && nested.CanBeTargeted)
			{
				return nested;
			}

			current = current.GetParent();
		}

		return null;
	}

	private bool TryGetMouseAimDirection(out Vector3 direction)
	{
		direction = Vector3.Zero;
		Viewport? viewport = GetViewport();
		Camera3D? camera = viewport?.GetCamera3D();
		if (viewport == null || camera == null || _owner == null)
		{
			return false;
		}

		Vector2 mousePosition = viewport.GetMousePosition();
		Vector3 rayOrigin = camera.ProjectRayOrigin(mousePosition);
		Vector3 rayDirection = camera.ProjectRayNormal(mousePosition);
		if (Mathf.Abs(rayDirection.Y) < 0.0001f)
		{
			return false;
		}

		float distance = (_owner.GlobalPosition.Y - rayOrigin.Y) / rayDirection.Y;
		if (distance <= 0.0f)
		{
			return false;
		}

		Vector3 targetPoint = rayOrigin + rayDirection * distance;
		direction = targetPoint - _owner.GlobalPosition;
		direction.Y = 0.0f;
		if (direction == Vector3.Zero)
		{
			return false;
		}

		direction = direction.Normalized();
		return true;
	}
}
