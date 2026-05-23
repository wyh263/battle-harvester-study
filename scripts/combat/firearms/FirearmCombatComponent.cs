using Godot;
using BattleHarvesterStudy.Attributes;
using BattleHarvesterStudy.Items;
using BattleHarvesterStudy.Equipment;
using BattleHarvesterStudy.Combat;
using BattleHarvesterStudy.Presentation;
using BattleHarvesterStudy.Targeting;
using BattleHarvesterStudy.Inventory;

namespace BattleHarvesterStudy.Combat.Firearms;

public partial class FirearmCombatComponent : Node
{
	private const double RecoilRecoveryDelaySeconds = 0.30;
	private const float RecoilRecoveryPerSecond = 1.8f;
	private const float MovingThreshold = 0.15f;

	private Node3D? _owner;
	private CharacterBody3D? _body;
	private StatsComponent? _stats;
	private EquipmentComponent? _equipment;
	private InventoryComponent? _inventory;
	private ActorSkillResourceController? _resources;
	private CombatAimController? _aimController;
	private GameplayInputGate? _gameplayInputGate;

	private string _trackedTargetId = string.Empty;
	private float _aimAccumulationProgress;
	private float _currentRecoilMultiplier = 1.0f;
	private double _shotCooldownRemaining;
	private double _timeSinceLastShot = 999.0;
	private double _reloadRemaining;
	private bool _wasPrimaryPressed;
	private Vector3 _currentAimDirection = Vector3.Zero;

	public bool IsAiming { get; private set; }
	public bool IsReloading => _reloadRemaining > 0.0;
	public float CurrentHitChance { get; private set; }
	public Node3D? CurrentTarget { get; private set; }
	public float CurrentRangeDamageMultiplier { get; private set; } = 1.0f;
	public float CurrentBaseHitComponent { get; private set; }
	public float CurrentAimBaseBonusComponent { get; private set; }
	public float CurrentAimPrecisionBonusComponent { get; private set; }
	public float CurrentHipFireBonusComponent { get; private set; }
	public float CurrentTargetMovingPenaltyComponent { get; private set; }
	public float CurrentRangePenaltyComponent { get; private set; }
	public float CurrentSelfMovingPenaltyComponent { get; private set; }
	public float CurrentDirectHitChance { get; private set; }
	public float CurrentRecoilMultiplier => _currentRecoilMultiplier;
	public float CurrentTargetDistance { get; private set; }
	public FirearmRangeBand CurrentRangeBand { get; private set; } = FirearmRangeBand.None;
	public bool CurrentOwnerMoving { get; private set; }
	public bool CurrentTargetMoving { get; private set; }
	public float CurrentConeAngleDegrees { get; private set; }

	public override void _Ready()
	{
		_owner = GetOwner<Node3D>();
		_body = _owner as CharacterBody3D;
		_stats = _owner?.GetNodeOrNull<StatsComponent>("Components/Stats");
		_equipment = _owner?.GetNodeOrNull<EquipmentComponent>("Components/Equipment");
		_inventory = _owner?.GetNodeOrNull<InventoryComponent>("Components/Inventory");
		_resources = _owner?.GetNodeOrNull<ActorSkillResourceController>("Components/SkillResources");
		_aimController = _owner?.GetNodeOrNull<CombatAimController>("Components/CombatAimController");
		_gameplayInputGate = _owner?.GetNodeOrNull<GameplayInputGate>("Components/GameplayInputGate");
		SetPhysicsProcess(true);
	}

	public override void _PhysicsProcess(double delta)
	{
		ShotCooldownTick(delta);
		RecoverRecoil(delta);

		FirearmWeaponDefinition? firearm = _equipment?.GetActiveFirearmDefinition();
		ItemInstance? firearmItem = _equipment?.GetActiveWeaponItem();
		if (_owner == null || _stats == null || _equipment == null || firearm == null || firearmItem == null)
		{
			ResetRuntimeState(clearAimController: true);
			return;
		}

		FirearmResolvedStats resolved = FirearmStatResolver.Resolve(firearm, firearmItem);

		if (_gameplayInputGate?.BlocksCombatInput ?? false)
		{
			CurrentHitChance = 0.0f;
			return;
		}

		UpdateReload(delta, firearmItem, resolved);
		UpdateAimState(delta, resolved);
		UpdateCurrentTarget(resolved);
		UpdateAimAccumulation(delta, resolved);
		UpdateDisplayedHitChance(firearm, firearmItem, resolved);
		HandleFireInput(firearm, firearmItem, resolved);
	}

	private void ShotCooldownTick(double delta)
	{
		if (_shotCooldownRemaining > 0.0)
		{
			_shotCooldownRemaining = Mathf.Max(0.0f, (float)(_shotCooldownRemaining - delta));
		}

		_timeSinceLastShot += delta;
	}

	private void RecoverRecoil(double delta)
	{
		if (_timeSinceLastShot < RecoilRecoveryDelaySeconds)
		{
			return;
		}

		_currentRecoilMultiplier = Mathf.MoveToward(_currentRecoilMultiplier, 1.0f, RecoilRecoveryPerSecond * (float)delta);
	}

	private void UpdateReload(double delta, ItemInstance firearmItem, FirearmResolvedStats resolved)
	{
		if (_reloadRemaining > 0.0)
		{
			_reloadRemaining = Mathf.Max(0.0f, (float)(_reloadRemaining - delta));
			if (_reloadRemaining <= 0.0)
			{
				FirearmAmmoInventoryService.TryReloadBestAvailable(_inventory, firearmItem, resolved, out _);
			}
		}
	}

	private void UpdateAimState(double delta, FirearmResolvedStats resolved)
	{
		bool aimPressed = Input.IsMouseButtonPressed(MouseButton.Right);
		bool canAimWhileMoving = resolved.CanAimWhileMoving || !IsOwnerMoving();
		bool hasStamina = _resources?.CurrentResource > 0.0f;
		bool shouldAim = aimPressed && canAimWhileMoving && hasStamina && !IsReloading;
		if (shouldAim && _resources != null)
		{
			float drain = resolved.GetAimStaminaDrainPerSecond() * (float)delta;
			if (!_resources.TrySpend(drain))
			{
				shouldAim = false;
			}
		}

		if (!shouldAim && IsAiming)
		{
			ClearAimAccumulation();
		}

		IsAiming = shouldAim;
	}

	private void UpdateCurrentTarget(FirearmResolvedStats resolved)
	{
		CurrentTarget = null;
		CurrentConeAngleDegrees = IsAiming ? resolved.AimConeAngleDegrees : resolved.HipFireConeAngleDegrees;
		if (!TryGetMouseAimDirection(out Vector3 aimDirection))
		{
			_currentAimDirection = Vector3.Zero;
			_aimController?.SetLockedTarget(null);
			return;
		}

		_currentAimDirection = aimDirection;
		_aimController?.SetAimOverrideDirection(aimDirection);
		Targetable? selectedTarget = FindBestTargetInCone(resolved, aimDirection);
		if (selectedTarget != null)
		{
			_aimController?.SetLockedTarget(selectedTarget);
			CurrentTarget = selectedTarget.GetTargetNode();
			return;
		}

		_aimController?.SetLockedTarget(null);
	}

	private void UpdateAimAccumulation(double delta, FirearmResolvedStats resolved)
	{
		if (!IsAiming || CurrentTarget == null)
		{
			ClearAimAccumulation();
			return;
		}

		string targetId = CurrentTarget.GetInstanceId().ToString();
		if (_trackedTargetId != targetId)
		{
			ClearAimAccumulation();
			_trackedTargetId = targetId;
		}

		float maxAimBonus = resolved.GetMaxAimBonus();
		if (maxAimBonus <= 0.0f)
		{
			_aimAccumulationProgress = 0.0f;
			return;
		}

		float duration = Mathf.Max(0.1f, resolved.GetAimAccumulationDuration());
		float accumulatePerSecond = 1.0f / duration;
		_aimAccumulationProgress = Mathf.Clamp(_aimAccumulationProgress + accumulatePerSecond * (float)delta, 0.0f, 1.0f);
	}

	private void UpdateDisplayedHitChance(FirearmWeaponDefinition firearm, ItemInstance firearmItem, FirearmResolvedStats resolved)
	{
		float rangeDamageMultiplier = 1.0f;
		ClearDisplayedBreakdown();
		CurrentHitChance = CurrentTarget == null
			? 0.0f
			: ResolveHitChance(firearm, firearmItem, resolved, CurrentTarget, out rangeDamageMultiplier);
		CurrentRangeDamageMultiplier = CurrentTarget == null ? 1.0f : rangeDamageMultiplier;
	}

	private void HandleFireInput(FirearmWeaponDefinition firearm, ItemInstance firearmItem, FirearmResolvedStats resolved)
	{
		bool primaryPressed = Input.IsMouseButtonPressed(MouseButton.Left);
		bool justPressed = primaryPressed && !_wasPrimaryPressed;
		_wasPrimaryPressed = primaryPressed;
		if (!ShouldFireThisFrame(resolved, primaryPressed, justPressed) || _shotCooldownRemaining > 0.0 || IsReloading)
		{
			return;
		}

		if (firearmItem.HasMagazine && firearmItem.CurrentMagazineAmmo <= 0)
		{
			StartReload(resolved);
			return;
		}

		if (CurrentTarget == null)
		{
			return;
		}

		if (!firearmItem.TryConsumeMagazineAmmo())
		{
			StartReload(resolved);
			return;
		}

		FireAtTarget(firearm, firearmItem, resolved, CurrentTarget, CurrentHitChance, CurrentRangeDamageMultiplier);
		_shotCooldownRemaining = 1.0 / Mathf.Max(0.01f, resolved.FireRate);
		_timeSinceLastShot = 0.0;
		_currentRecoilMultiplier = Mathf.Max(
			resolved.GetRecoilFloorMultiplier(),
			_currentRecoilMultiplier - resolved.GetRecoilStepMultiplier());

		if (firearmItem.CurrentMagazineAmmo <= 0)
		{
			StartReload(resolved);
		}
	}

	private static bool ShouldFireThisFrame(FirearmResolvedStats resolved, bool primaryPressed, bool justPressed)
	{
		return resolved.FireMode switch
		{
			FirearmFireMode.Automatic => primaryPressed,
			FirearmFireMode.Selective => primaryPressed,
			_ => justPressed
		};
	}

	private void FireAtTarget(FirearmWeaponDefinition firearm, ItemInstance firearmItem, FirearmResolvedStats resolved, Node3D target, float hitChance, float damageMultiplier)
	{
		Hurtbox? hurtbox = target.GetNodeOrNull<Hurtbox>("Hurtbox");
		if (hurtbox == null || _owner == null)
		{
			return;
		}

		Vector3 hitDirection = (target.GlobalPosition - _owner.GlobalPosition).Normalized();
		for (int pelletIndex = 0; pelletIndex < Mathf.Max(1, resolved.PelletCount); pelletIndex++)
		{
			if (GD.Randf() * 100.0f > hitChance)
			{
				continue;
			}

			int pelletDamage = Mathf.Max(1, Mathf.RoundToInt(resolved.BaseDamagePerPellet * damageMultiplier));
			DamageCalculationResult calculatedDamage = CombatStatFormulas.CalculateDamage(new DamageCalculationInput
			{
				BaseDamage = pelletDamage,
				AttackPowerScaling = 0.0f,
				MagicPowerScaling = 0.0f,
				School = firearm.DamageSchool,
				DeliveryModel = DamageDeliveryModel.Direct,
				CanCrit = true,
				SkillMultiplier = 1.0f,
				SkillPowerTier = firearmItem.LoadedAmmoPenetrationTier,
				DestructionMultiplier = 1.0f,
				BypassArmorPoint = firearm.GrantsBypassArmorPoint,
				ActiveWeapon = firearm,
				WeaponConditionDamageMultiplier = firearmItem.CurrentDurability <= 0.0f ? firearm.BrokenDamageMultiplier : 1.0f,
				AttackerStats = _stats,
				DefenderStats = target.GetNodeOrNull<StatsComponent>("Components/Stats"),
				DefenderArmor = target.GetNodeOrNull<ArmorComponent>("Components/Armor")
			});

			hurtbox.TakeDamage(new DamageInfo(
				calculatedDamage.FinalDamage,
				hitDirection * 1.5f,
				_owner,
				attackId: firearm.DefinitionName(),
				causesForcedMovement: false,
				baseAmount: calculatedDamage.BaseDamage,
				wasCritical: calculatedDamage.IsCritical,
				rawAmount: calculatedDamage.RawDamage,
				tierAdjustedAmount: calculatedDamage.TierAdjustedDamage,
				armorAbsorbed: calculatedDamage.ArmorAbsorbed,
				postArmorAmount: calculatedDamage.PostArmorDamage,
				tierDelta: calculatedDamage.TierDelta,
				school: firearm.DamageSchool,
				deliveryModel: DamageDeliveryModel.Direct,
				durabilityDamage: calculatedDamage.DurabilityDamage,
				penetrationEffectiveness: calculatedDamage.PenetrationEffectiveness,
				armorLossResistance: calculatedDamage.ArmorLossResistance,
				effectiveAbsorbRate: calculatedDamage.EffectiveAbsorbRate));
		}

		if (firearmItem.HasDurability)
		{
			firearmItem.ApplyDurabilityDamage(firearm.GetDurabilityLoss(CombatActionType.Light));
		}
	}

	private float ResolveHitChance(
		FirearmWeaponDefinition firearm,
		ItemInstance firearmItem,
		FirearmResolvedStats resolved,
		Node3D target,
		out float rangeDamageMultiplier)
	{
		rangeDamageMultiplier = 1.0f;
		bool ownerMoving = IsOwnerMoving();
		CurrentOwnerMoving = ownerMoving;
		CurrentBaseHitComponent = resolved.BaseHitChance;
		CurrentAimBaseBonusComponent = IsAiming ? resolved.AimBaseBonus : 0.0f;
		CurrentAimPrecisionBonusComponent = IsAiming ? GetCurrentAimPrecisionBonus(resolved) : 0.0f;
		CurrentHipFireBonusComponent = IsAiming ? 0.0f : resolved.GetHipFireBonus();
		float directHit = IsAiming ? ResolveAimDirectHit(resolved) : ResolveHipFireDirectHit(resolved);

		CurrentTargetMoving = IsTargetMoving(target);
		if (CurrentTargetMoving)
		{
			CurrentTargetMovingPenaltyComponent = resolved.TargetMovingPenalty;
			directHit -= resolved.TargetMovingPenalty;
		}

		float distance = _owner?.GlobalPosition.DistanceTo(target.GlobalPosition) ?? 0.0f;
		CurrentTargetDistance = distance;
		CurrentRangeBand = FirearmRangeBand.Effective;
		if (distance > resolved.SevereFalloffRange)
		{
			CurrentRangeBand = FirearmRangeBand.Severe;
			CurrentRangePenaltyComponent = resolved.SevereRangeHitPenalty;
			directHit -= resolved.SevereRangeHitPenalty;
			rangeDamageMultiplier = resolved.SevereRangeDamageMultiplier;
		}
		else if (distance > resolved.EffectiveRange)
		{
			CurrentRangeBand = FirearmRangeBand.Falloff;
			CurrentRangePenaltyComponent = resolved.FalloffRangeHitPenalty;
			directHit -= resolved.FalloffRangeHitPenalty;
			rangeDamageMultiplier = resolved.FalloffRangeDamageMultiplier;
		}

		if (ownerMoving)
		{
			CurrentSelfMovingPenaltyComponent = IsAiming ? resolved.MovingAimPenalty : resolved.GetHipFireMovingPenalty();
			directHit -= CurrentSelfMovingPenaltyComponent;
		}

		float conditionMultiplier = firearmItem.CurrentDurability <= 0.0f ? firearm.BrokenDamageMultiplier : 1.0f;
		rangeDamageMultiplier *= conditionMultiplier;
		CurrentDirectHitChance = directHit;
		return Mathf.Max(0.0f, directHit * _currentRecoilMultiplier);
	}

	private float ResolveAimDirectHit(FirearmResolvedStats resolved)
	{
		return resolved.BaseHitChance + resolved.AimBaseBonus + GetCurrentAimPrecisionBonus(resolved);
	}

	private static float ResolveHipFireDirectHit(FirearmResolvedStats resolved)
	{
		return resolved.BaseHitChance + resolved.GetHipFireBonus();
	}

	private bool IsTargetInFirearmRange(Node3D target, FirearmResolvedStats resolved)
	{
		if (_owner == null)
		{
			return false;
		}

		return _owner.GlobalPosition.DistanceTo(target.GlobalPosition) <= resolved.SevereFalloffRange * 1.25f;
	}

	private Targetable? FindBestTargetInCone(FirearmResolvedStats resolved, Vector3 aimDirection)
	{
		if (_owner == null)
		{
			return null;
		}

		SceneTree? tree = GetTree();
		if (tree == null)
		{
			return null;
		}

		float bestDistanceSquared = float.MaxValue;
		Targetable? bestTarget = null;
		float halfConeRadians = Mathf.DegToRad(CurrentConeAngleDegrees * 0.5f);
		float minDot = Mathf.Cos(halfConeRadians);

		foreach (Node node in tree.GetNodesInGroup(Targetable.TargetableGroupName))
		{
			if (node is not Targetable targetable || !targetable.CanBeTargeted)
			{
				continue;
			}

			Node3D? targetNode = targetable.GetTargetNode();
			if (targetNode == null || targetNode == _owner || !IsTargetInFirearmRange(targetNode, resolved))
			{
				continue;
			}

			Vector3 toTarget = targetNode.GlobalPosition - _owner.GlobalPosition;
			toTarget.Y = 0.0f;
			if (toTarget == Vector3.Zero)
			{
				continue;
			}

			Vector3 targetDirection = toTarget.Normalized();
			if (aimDirection.Dot(targetDirection) < minDot)
			{
				continue;
			}

			float distanceSquared = toTarget.LengthSquared();
			if (distanceSquared < bestDistanceSquared)
			{
				bestDistanceSquared = distanceSquared;
				bestTarget = targetable;
			}
		}

		return bestTarget;
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

	private bool IsOwnerMoving()
	{
		Vector3 velocity = _body?.Velocity ?? Vector3.Zero;
		velocity.Y = 0.0f;
		return velocity.Length() > MovingThreshold;
	}

	private static bool IsTargetMoving(Node3D target)
	{
		if (target is not CharacterBody3D body)
		{
			return false;
		}

		Vector3 velocity = body.Velocity;
		velocity.Y = 0.0f;
		return velocity.Length() > MovingThreshold;
	}

	private void StartReload(FirearmResolvedStats resolved)
	{
		if (IsReloading)
		{
			return;
		}

		if (FirearmAmmoInventoryService.GetReserveAmmoCount(_inventory, resolved.AmmoType) <= 0)
		{
			return;
		}

		_reloadRemaining = Mathf.Max(0.25f, resolved.ReloadDuration);
	}

	private void ClearAimAccumulation()
	{
		_trackedTargetId = string.Empty;
		_aimAccumulationProgress = 0.0f;
	}

	private void ResetRuntimeState(bool clearAimController)
	{
		IsAiming = false;
		CurrentHitChance = 0.0f;
		CurrentRangeDamageMultiplier = 1.0f;
		CurrentTarget = null;
		_reloadRemaining = 0.0;
		_currentAimDirection = Vector3.Zero;
		ClearAimAccumulation();
		ClearDisplayedBreakdown();
		if (clearAimController)
		{
			_aimController?.EnterFreeState();
		}
	}

	private float GetCurrentAimPrecisionBonus(FirearmResolvedStats resolved)
	{
		return resolved.GetMaxAimBonus() * _aimAccumulationProgress;
	}

	private void ClearDisplayedBreakdown()
	{
		CurrentBaseHitComponent = 0.0f;
		CurrentAimBaseBonusComponent = 0.0f;
		CurrentAimPrecisionBonusComponent = 0.0f;
		CurrentHipFireBonusComponent = 0.0f;
		CurrentTargetMovingPenaltyComponent = 0.0f;
		CurrentRangePenaltyComponent = 0.0f;
		CurrentSelfMovingPenaltyComponent = 0.0f;
		CurrentDirectHitChance = 0.0f;
		CurrentTargetDistance = 0.0f;
		CurrentRangeBand = FirearmRangeBand.None;
		CurrentOwnerMoving = false;
		CurrentTargetMoving = false;
		CurrentConeAngleDegrees = 0.0f;
	}
}

internal static class FirearmWeaponDefinitionExtensions
{
	public static string DefinitionName(this FirearmWeaponDefinition firearm)
	{
		return $"{firearm.WeaponFamilyId}_{firearm.Tier}";
	}
}
