using Godot;
using System;
using System.Collections.Generic;

namespace BattleHarvesterStudy.Items;

public sealed class ItemInstance
{
	public ItemInstance(ItemDefinition definition, int stackCount = 1, bool isRotated = false, string? instanceId = null, int? remainingUses = null)
	{
		Definition = definition ?? throw new ArgumentNullException(nameof(definition));
		InstanceId = string.IsNullOrWhiteSpace(instanceId) ? Guid.NewGuid().ToString("N") : instanceId;
		StackCount = Mathf.Clamp(stackCount, 1, Mathf.Max(1, definition.MaxStack));
		IsRotated = definition.CanRotate && isRotated;
		RemainingUses = ResolveInitialRemainingUses(definition, remainingUses);
		CurrentMaxDurability = ResolveInitialMaxDurability(definition);
		CurrentDurability = CurrentMaxDurability;
		CurrentMaxArmorPoint = ResolveInitialMaxArmorPoint(definition);
		CurrentArmorPoint = CurrentMaxArmorPoint;
		CurrentMagazineAmmo = ResolveInitialMagazineAmmo(definition);
		InitializeWeaponSkillSlots(definition);
		InitializeLoadedAmmo(definition);
	}

	public string InstanceId { get; }
	public ItemDefinition Definition { get; }
	public int StackCount { get; private set; }
	public bool IsRotated { get; private set; }
	public int RemainingUses { get; private set; }
	public bool HasLimitedUses => Definition.Usable?.UsesPerItem > 0;
	public float CurrentDurability { get; private set; }
	public float CurrentMaxDurability { get; private set; }
	public float CurrentArmorPoint { get; private set; }
	public float CurrentMaxArmorPoint { get; private set; }
	public int CurrentMagazineAmmo { get; private set; }
	public string LoadedAmmoItemId { get; private set; } = string.Empty;
	public AmmoType LoadedAmmoType { get; private set; } = AmmoType.None;
	public int LoadedAmmoTier { get; private set; }
	public float LoadedAmmoPenetrationTier { get; private set; }
	public ItemAcquisitionState AcquisitionState { get; private set; }
	public int RunLootStackCount { get; private set; }
	public bool HasDurability => CurrentMaxDurability > 0.0f;
	public bool HasArmorPoint => CurrentMaxArmorPoint > 0.0f;
	public bool HasMagazine => ResolveMagazineCapacity(Definition) > 0;
	public bool HasWeaponSkillSlots => _weaponSkillSlots.Count > 0;
	public IReadOnlyList<InstalledWeaponSkillState> WeaponSkillSlots => _weaponSkillSlots;
	public bool CountsAsRunLoot => RunLootStackCount > 0;

	private readonly List<InstalledWeaponSkillState> _weaponSkillSlots = [];

	public int GetAvailableStackSpace()
	{
		return Mathf.Max(0, Definition.MaxStack - StackCount);
	}

	public int GetFootprintWidth()
	{
		return IsRotated ? Definition.GridHeight : Definition.GridWidth;
	}

	public int GetFootprintHeight()
	{
		return IsRotated ? Definition.GridWidth : Definition.GridHeight;
	}

	public Vector2I GetFootprint()
	{
		return new Vector2I(GetFootprintWidth(), GetFootprintHeight());
	}

	public bool CanStackWith(ItemInstance other)
	{
		return other.Definition.ItemId == Definition.ItemId
			&& Definition.MaxStack > 1
			&& !HasLimitedUses
			&& !other.HasLimitedUses
			&& !IsRotated
			&& !other.IsRotated;
	}

	public bool TryAddToStack(int amount, out int addedAmount)
	{
		addedAmount = 0;
		if (amount <= 0 || Definition.MaxStack <= 1)
		{
			return false;
		}

		addedAmount = Mathf.Min(amount, GetAvailableStackSpace());
		if (addedAmount <= 0)
		{
			return false;
		}

		StackCount += addedAmount;
		return true;
	}

	public bool TrySetStackCount(int stackCount)
	{
		int clamped = Mathf.Clamp(stackCount, 1, Mathf.Max(1, Definition.MaxStack));
		if (clamped == StackCount)
		{
			return false;
		}

		StackCount = clamped;
		RunLootStackCount = Mathf.Clamp(RunLootStackCount, 0, StackCount);
		SyncAcquisitionStateFromRunLootCount();
		return true;
	}

	public int GetBaseStackCount()
	{
		return Mathf.Max(0, StackCount - RunLootStackCount);
	}

	public int PeekRunLootAmountInConsumedStack(int amount)
	{
		int clampedAmount = Mathf.Clamp(amount, 0, StackCount);
		int baseAmount = GetBaseStackCount();
		return Mathf.Max(0, clampedAmount - baseAmount);
	}

	public void AddRunLootToStack(int amount)
	{
		if (amount <= 0)
		{
			return;
		}

		RunLootStackCount = Mathf.Clamp(RunLootStackCount + amount, 0, StackCount);
		SyncAcquisitionStateFromRunLootCount();
	}

	public bool TryConsumeStackAmount(int amount)
	{
		if (amount <= 0 || amount > StackCount)
		{
			return false;
		}

		int consumedRunLoot = PeekRunLootAmountInConsumedStack(amount);
		StackCount -= amount;
		RunLootStackCount = Mathf.Clamp(RunLootStackCount - consumedRunLoot, 0, StackCount);
		SyncAcquisitionStateFromRunLootCount();
		return true;
	}

	public bool TryConsumeUse(out bool depleted)
	{
		depleted = false;
		if (!HasLimitedUses || RemainingUses <= 0)
		{
			return false;
		}

		RemainingUses--;
		depleted = RemainingUses <= 0;
		return true;
	}

	public float ApplyDurabilityDamage(float amount)
	{
		if (amount <= 0.0f || CurrentDurability <= 0.0f)
		{
			return 0.0f;
		}

		float applied = Mathf.Min(CurrentDurability, amount);
		CurrentDurability -= applied;
		return applied;
	}

	public float ApplyDurabilityMaxDamage(float amount)
	{
		if (amount <= 0.0f || CurrentMaxDurability <= 0.0f)
		{
			return 0.0f;
		}

		float applied = Mathf.Min(CurrentMaxDurability, amount);
		CurrentMaxDurability -= applied;
		CurrentDurability = Mathf.Clamp(CurrentDurability, 0.0f, CurrentMaxDurability);
		return applied;
	}

	public float ApplyArmorPointDamage(float amount)
	{
		if (amount <= 0.0f || CurrentArmorPoint <= 0.0f)
		{
			return 0.0f;
		}

		float applied = Mathf.Min(CurrentArmorPoint, amount);
		CurrentArmorPoint -= applied;
		return applied;
	}

	public float ApplyArmorPointMaxDamage(float amount)
	{
		if (amount <= 0.0f || CurrentMaxArmorPoint <= 0.0f)
		{
			return 0.0f;
		}

		float applied = Mathf.Min(CurrentMaxArmorPoint, amount);
		CurrentMaxArmorPoint -= applied;
		CurrentArmorPoint = Mathf.Clamp(CurrentArmorPoint, 0.0f, CurrentMaxArmorPoint);
		return applied;
	}

	public void RefillArmorPoint(float maxMultiplier = 1.0f)
	{
		if (CurrentMaxArmorPoint <= 0.0f)
		{
			return;
		}

		CurrentMaxArmorPoint = Mathf.Max(0.0f, CurrentMaxArmorPoint * Mathf.Clamp(maxMultiplier, 0.0f, 1.0f));
		CurrentArmorPoint = CurrentMaxArmorPoint;
	}

	public int GetMagazineCapacity()
	{
		return ResolveMagazineCapacity(Definition);
	}

	public bool TryConsumeMagazineAmmo(int amount = 1)
	{
		if (!HasMagazine)
		{
			return false;
		}

		int clampedAmount = Mathf.Max(1, amount);
		if (CurrentMagazineAmmo < clampedAmount)
		{
			return false;
		}

		CurrentMagazineAmmo -= clampedAmount;
		return true;
	}

	public void LoadMagazine(int ammoCount, AmmoItemDefinition ammoDefinition, string loadedAmmoItemId)
	{
		LoadMagazine(ammoCount, ammoDefinition, loadedAmmoItemId, GetMagazineCapacity());
	}

	public void LoadMagazine(int ammoCount, AmmoItemDefinition ammoDefinition, string loadedAmmoItemId, int magazineCapacity)
	{
		CurrentMagazineAmmo = Mathf.Clamp(ammoCount, 0, Mathf.Max(0, magazineCapacity));
		LoadedAmmoItemId = loadedAmmoItemId ?? string.Empty;
		LoadedAmmoType = ammoDefinition.AmmoType;
		LoadedAmmoTier = ammoDefinition.AmmoTier;
		LoadedAmmoPenetrationTier = ammoDefinition.PenetrationTier;
		if (CurrentMagazineAmmo <= 0)
		{
			ClearLoadedAmmo();
		}
	}

	public void ClearLoadedAmmo()
	{
		LoadedAmmoItemId = string.Empty;
		LoadedAmmoType = AmmoType.None;
		LoadedAmmoTier = 0;
		LoadedAmmoPenetrationTier = 0.0f;
	}

	public AmmoItemDefinition? GetLoadedAmmoDefinition()
	{
		if (LoadedAmmoType == AmmoType.None || LoadedAmmoTier <= 0)
		{
			return null;
		}

		return new AmmoItemDefinition
		{
			AmmoType = LoadedAmmoType,
			AmmoTier = LoadedAmmoTier,
			PenetrationTier = LoadedAmmoPenetrationTier
		};
	}

	public int GetWeaponSkillSlotCount()
	{
		return _weaponSkillSlots.Count;
	}

	public InstalledWeaponSkillState? GetInstalledWeaponSkillState(int slotIndex)
	{
		return slotIndex >= 0 && slotIndex < _weaponSkillSlots.Count
			? _weaponSkillSlots[slotIndex]
			: null;
	}

	public SkillDefinition? GetInstalledWeaponSkillDefinition(int slotIndex)
	{
		return GetInstalledWeaponSkillState(slotIndex)?.ResolveCastableSkill();
	}

	public bool TryInstallWeaponSkill(
		WeaponSkillItemDefinition weaponSkillItem,
		int remainingUses,
		ItemDefinition? sourceItemDefinition,
		ItemAcquisitionState sourceAcquisitionState,
		out int installedSlotIndex)
	{
		installedSlotIndex = -1;
		if (weaponSkillItem.Skill == null || _weaponSkillSlots.Count == 0)
		{
			return false;
		}

		for (int index = 0; index < _weaponSkillSlots.Count; index++)
		{
			if (!_weaponSkillSlots[index].IsEmpty)
			{
				continue;
			}

			if (TryInstallWeaponSkillState(weaponSkillItem.CreateInstalledState(index, sourceItemDefinition, sourceAcquisitionState, remainingUses), index, false, out _))
			{
				installedSlotIndex = index;
				return true;
			}
		}

		return false;
	}

	public bool TryInstallWeaponSkillAtSlot(
		WeaponSkillItemDefinition weaponSkillItem,
		int remainingUses,
		ItemDefinition? sourceItemDefinition,
		ItemAcquisitionState sourceAcquisitionState,
		int slotIndex,
		bool replaceExisting,
		out InstalledWeaponSkillState? replacedState)
	{
		replacedState = null;
		return weaponSkillItem.Skill != null
			&& TryInstallWeaponSkillState(weaponSkillItem.CreateInstalledState(slotIndex, sourceItemDefinition, sourceAcquisitionState, remainingUses), slotIndex, replaceExisting, out replacedState);
	}

	public bool TryClearWeaponSkillSlot(int slotIndex, out InstalledWeaponSkillState? removedState)
	{
		removedState = null;
		if (slotIndex < 0 || slotIndex >= _weaponSkillSlots.Count || _weaponSkillSlots[slotIndex].IsEmpty)
		{
			return false;
		}

		removedState = _weaponSkillSlots[slotIndex].CreateCopy();
		_weaponSkillSlots[slotIndex] = InstalledWeaponSkillState.CreateEmpty(slotIndex);
		return true;
	}

	public bool TryEquipLearnedWeaponSkill(SkillDefinition skill, int slotIndex, bool replaceExisting)
	{
		return skill.LoadoutCategory == SkillLoadoutCategory.Weapon
			&& TryInstallWeaponSkillState(InstalledWeaponSkillState.CreateLearned(slotIndex, skill), slotIndex, replaceExisting, out _);
	}

	public bool TryRestoreWeaponSkillState(InstalledWeaponSkillState state, bool replaceExisting)
	{
		return TryInstallWeaponSkillState(state, state.SlotIndex, replaceExisting, out _);
	}

	private bool TryInstallWeaponSkillState(
		InstalledWeaponSkillState state,
		int slotIndex,
		bool replaceExisting,
		out InstalledWeaponSkillState? replacedState)
	{
		replacedState = null;
		SkillDefinition? skill = state.Skill;
		if (skill == null || slotIndex < 0 || slotIndex >= _weaponSkillSlots.Count)
		{
			return false;
		}

		for (int index = 0; index < _weaponSkillSlots.Count; index++)
		{
			if (index == slotIndex)
			{
				continue;
			}

			if (_weaponSkillSlots[index].MatchesSkill(skill))
			{
				return false;
			}
		}

		InstalledWeaponSkillState currentState = _weaponSkillSlots[slotIndex];
		if (!currentState.IsEmpty && !replaceExisting)
		{
			return false;
		}

		replacedState = currentState.IsEmpty ? null : currentState.CreateCopy();
		_weaponSkillSlots[slotIndex] = state.CreateCopy();
		return true;
	}

	public bool NotifyInstalledWeaponSkillCast(SkillDefinition skill, out InstalledWeaponSkillState? depletedState)
	{
		depletedState = null;
		foreach (InstalledWeaponSkillState state in _weaponSkillSlots)
		{
			if (!state.MatchesSkill(skill))
			{
				continue;
			}

			bool recorded = state.RecordCast(out bool depleted);
			if (recorded && depleted)
			{
				depletedState = state.CreateCopy();
				_weaponSkillSlots[state.SlotIndex] = InstalledWeaponSkillState.CreateEmpty(state.SlotIndex);
			}

			return recorded;
		}

		return false;
	}

	public bool NotifyInstalledWeaponSkillKill(SkillDefinition skill, bool bossKill)
	{
		foreach (InstalledWeaponSkillState state in _weaponSkillSlots)
		{
			if (!state.MatchesSkill(skill))
			{
				continue;
			}

			state.RecordKill(bossKill);
			return true;
		}

		return false;
	}

	public bool Rotate()
	{
		if (!Definition.CanRotate || Definition.GridWidth == Definition.GridHeight)
		{
			return false;
		}

		IsRotated = !IsRotated;
		return true;
	}

	public bool TrySetRotation(bool rotated)
	{
		if (IsRotated == rotated)
		{
			return true;
		}

		return Rotate();
	}

	public ItemInstance CreateCopy(bool preserveInstanceId = false)
	{
		ItemInstance copy = new(Definition, StackCount, IsRotated, preserveInstanceId ? InstanceId : null, RemainingUses)
		{
			CurrentDurability = CurrentDurability,
			CurrentMaxDurability = CurrentMaxDurability,
			CurrentArmorPoint = CurrentArmorPoint,
			CurrentMaxArmorPoint = CurrentMaxArmorPoint,
			CurrentMagazineAmmo = CurrentMagazineAmmo,
			LoadedAmmoItemId = LoadedAmmoItemId,
			LoadedAmmoType = LoadedAmmoType,
			LoadedAmmoTier = LoadedAmmoTier,
			LoadedAmmoPenetrationTier = LoadedAmmoPenetrationTier,
			AcquisitionState = AcquisitionState,
			RunLootStackCount = RunLootStackCount,
		};
		copy.RestoreWeaponSkillSlots(_weaponSkillSlots);
		return copy;
	}

	public void SetAcquisitionState(ItemAcquisitionState state)
	{
		AcquisitionState = state;
		RunLootStackCount = state == ItemAcquisitionState.RunLoot ? StackCount : 0;
	}

	public void RestoreAcquisitionState(ItemAcquisitionState state, int runLootStackCount)
	{
		AcquisitionState = state;
		RunLootStackCount = Mathf.Clamp(runLootStackCount, 0, StackCount);
		SyncAcquisitionStateFromRunLootCount();
	}

	public void RestoreRuntimeState(
		float currentDurability,
		float currentMaxDurability,
		float currentArmorPoint,
		float currentMaxArmorPoint)
	{
		CurrentMaxDurability = Mathf.Max(0.0f, currentMaxDurability);
		CurrentDurability = Mathf.Clamp(currentDurability, 0.0f, CurrentMaxDurability);
		CurrentMaxArmorPoint = Mathf.Max(0.0f, currentMaxArmorPoint);
		CurrentArmorPoint = Mathf.Clamp(currentArmorPoint, 0.0f, CurrentMaxArmorPoint);
		CurrentMagazineAmmo = Mathf.Clamp(CurrentMagazineAmmo, 0, GetMagazineCapacity());
	}

	public void RestoreMagazineAmmo(int currentMagazineAmmo)
	{
		CurrentMagazineAmmo = Mathf.Clamp(currentMagazineAmmo, 0, GetMagazineCapacity());
	}

	public void RestoreLoadedAmmo(string loadedAmmoItemId, AmmoType loadedAmmoType, int loadedAmmoTier, float loadedAmmoPenetrationTier)
	{
		LoadedAmmoItemId = loadedAmmoItemId ?? string.Empty;
		LoadedAmmoType = loadedAmmoType;
		LoadedAmmoTier = loadedAmmoTier;
		LoadedAmmoPenetrationTier = loadedAmmoPenetrationTier;
		if (CurrentMagazineAmmo <= 0)
		{
			ClearLoadedAmmo();
		}
	}

	public void RestoreWeaponSkillSlots(IEnumerable<InstalledWeaponSkillState> slotStates)
	{
		_weaponSkillSlots.Clear();
		foreach (InstalledWeaponSkillState state in slotStates)
		{
			_weaponSkillSlots.Add(state.CreateCopy());
		}

		int expectedSlotCount = ResolveInitialWeaponSkillSlotCount(Definition);
		for (int index = _weaponSkillSlots.Count; index < expectedSlotCount; index++)
		{
			_weaponSkillSlots.Add(InstalledWeaponSkillState.CreateEmpty(index));
		}
	}

	private static int ResolveInitialRemainingUses(ItemDefinition definition, int? remainingUses)
	{
		if (remainingUses.HasValue)
		{
			return Mathf.Max(0, remainingUses.Value);
		}

		if (definition.Usable?.UsesPerItem > 0)
		{
			return Mathf.Max(0, definition.Usable.UsesPerItem);
		}

		if (definition.WeaponSkill != null)
		{
			return Mathf.Max(0, definition.WeaponSkill.GrantedUses);
		}

		return 0;
	}

	private static float ResolveInitialMaxDurability(ItemDefinition definition)
	{
		return definition.Equipment is WeaponEquipmentDefinition weapon
			? Mathf.Max(0.0f, weapon.MaxDurability)
			: 0.0f;
	}

	private static float ResolveInitialMaxArmorPoint(ItemDefinition definition)
	{
		return definition.Equipment is ArmorEquipmentDefinition armor
			? Mathf.Max(0.0f, armor.MaxArmorPoint)
			: 0.0f;
	}

	private static int ResolveInitialMagazineAmmo(ItemDefinition definition)
	{
		return ResolveMagazineCapacity(definition);
	}

	private static int ResolveMagazineCapacity(ItemDefinition definition)
	{
		return definition.Equipment is FirearmWeaponDefinition firearm
			? Mathf.Max(0, firearm.MagazineCapacity)
			: 0;
	}

	private static AmmoItemDefinition? ResolveInitialLoadedAmmo(ItemDefinition definition)
	{
		if (definition.Equipment is not FirearmWeaponDefinition firearm || firearm.MagazineCapacity <= 0)
		{
			return null;
		}

		return new AmmoItemDefinition
		{
			AmmoType = firearm.AmmoType,
			AmmoTier = Mathf.Max(1, firearm.Tier),
			PenetrationTier = Mathf.Max(1, firearm.Tier)
		};
	}

	private void InitializeWeaponSkillSlots(ItemDefinition definition)
	{
		_weaponSkillSlots.Clear();
		int slotCount = ResolveInitialWeaponSkillSlotCount(definition);
		for (int index = 0; index < slotCount; index++)
		{
			_weaponSkillSlots.Add(InstalledWeaponSkillState.CreateEmpty(index));
		}
	}

	private void InitializeLoadedAmmo(ItemDefinition definition)
	{
		AmmoItemDefinition? ammo = ResolveInitialLoadedAmmo(definition);
		if (ammo == null)
		{
			ClearLoadedAmmo();
			return;
		}

		LoadMagazine(
			CurrentMagazineAmmo,
			ammo,
			AmmoItemDefinition.BuildDefaultAmmoItemId(ammo.AmmoType, ammo.AmmoTier));
	}

	private static int ResolveInitialWeaponSkillSlotCount(ItemDefinition definition)
	{
		return definition.Equipment is WeaponEquipmentDefinition weapon
			? weapon.GetWeaponSkillSlotCount()
			: 0;
	}

	private void SyncAcquisitionStateFromRunLootCount()
	{
		AcquisitionState = RunLootStackCount > 0
			? ItemAcquisitionState.RunLoot
			: ItemAcquisitionState.Base;
	}
}
