using Godot;
using System.Collections.Generic;

namespace BattleHarvesterStudy.Equipment;

public partial class EquipmentComponent : Node
{
	[Signal]
	public delegate void EquipmentChangedEventHandler();

	[Signal]
	public delegate void ActiveWeaponSlotChangedEventHandler(int slotType);

	private readonly Dictionary<EquipmentSlotType, EquipmentSlotRecord> _slots = new();
	private readonly List<(string Key, StatType StatType)> _appliedModifierKeys = [];
	private StatsComponent? _statsComponent;
	private ActorSkillLoadout? _skillLoadout;
	private WeaponSkillKnowledgeComponent? _weaponSkillKnowledge;
	private EquipmentSlotType _activeWeaponSlot = EquipmentSlotType.WeaponSlot1;

	public override void _Ready()
	{
		Node3D? owner = GetOwner<Node3D>();
		_statsComponent = owner?.GetNodeOrNull<StatsComponent>("Components/Stats");
		_skillLoadout = owner?.GetNodeOrNull<ActorSkillLoadout>("Components/SkillLoadout");
		_weaponSkillKnowledge = owner?.GetNodeOrNull<WeaponSkillKnowledgeComponent>("Components/WeaponSkillKnowledge");
		EnsureDefaultSlots();
		RefreshEquipmentEffects();
	}

	public IReadOnlyDictionary<EquipmentSlotType, EquipmentSlotRecord> Slots => _slots;
	public EquipmentSlotType ActiveWeaponSlot => ResolveActiveWeaponSlot();

	public bool TryEquip(EquipmentSlotType slotType, ItemInstance item, out ItemInstance? replacedItem, out EquipmentActionFailureReason failureReason)
	{
		replacedItem = null;
		failureReason = EquipmentActionFailureReason.None;
		if (!_slots.TryGetValue(slotType, out EquipmentSlotRecord? slot))
		{
			failureReason = EquipmentActionFailureReason.MissingSlot;
			return false;
		}

		if (!CanEquip(slot, item, out failureReason))
		{
			return false;
		}

		if (slot.EquippedItem != null)
		{
			failureReason = EquipmentActionFailureReason.SlotOccupied;
			return false;
		}

		slot.EquippedItem = item;
		if (IsWeaponSlot(slotType) && !HasWeaponInSlot(_activeWeaponSlot))
		{
			_activeWeaponSlot = slotType;
		}

		RefreshEquipmentEffects();
		EmitSignal(SignalName.EquipmentChanged);
		return true;
	}

	public bool TryUnequip(EquipmentSlotType slotType, out ItemInstance? removedItem, out EquipmentActionFailureReason failureReason)
	{
		removedItem = null;
		failureReason = EquipmentActionFailureReason.None;
		if (!_slots.TryGetValue(slotType, out EquipmentSlotRecord? slot))
		{
			failureReason = EquipmentActionFailureReason.MissingSlot;
			return false;
		}

		if (slot.EquippedItem == null)
		{
			failureReason = EquipmentActionFailureReason.SlotEmpty;
			return false;
		}

		removedItem = slot.EquippedItem;
		slot.EquippedItem = null;
		if (slotType == _activeWeaponSlot)
		{
			_activeWeaponSlot = ResolveActiveWeaponSlot();
			EmitSignal(SignalName.ActiveWeaponSlotChanged, (int)_activeWeaponSlot);
		}

		RefreshEquipmentEffects();
		EmitSignal(SignalName.EquipmentChanged);
		return true;
	}

	public bool TryRearrangeEquippedItem(
		EquipmentSlotType sourceSlotType,
		EquipmentSlotType targetSlotType,
		out EquipmentActionFailureReason failureReason)
	{
		failureReason = EquipmentActionFailureReason.None;
		if (!_slots.TryGetValue(sourceSlotType, out EquipmentSlotRecord? sourceSlot)
			|| !_slots.TryGetValue(targetSlotType, out EquipmentSlotRecord? targetSlot))
		{
			failureReason = EquipmentActionFailureReason.MissingSlot;
			return false;
		}

		if (sourceSlot.EquippedItem == null)
		{
			failureReason = EquipmentActionFailureReason.SlotEmpty;
			return false;
		}

		if (sourceSlotType == targetSlotType)
		{
			return true;
		}

		ItemInstance sourceItem = sourceSlot.EquippedItem;
		ItemInstance? targetItem = targetSlot.EquippedItem;
		if (!CanEquip(targetSlot, sourceItem, out failureReason))
		{
			return false;
		}

		if (targetItem != null && !CanEquip(sourceSlot, targetItem, out failureReason))
		{
			return false;
		}

		sourceSlot.EquippedItem = targetItem;
		targetSlot.EquippedItem = sourceItem;

		EquipmentSlotType previousActiveWeaponSlot = _activeWeaponSlot;
		if (IsWeaponSlot(sourceSlotType) || IsWeaponSlot(targetSlotType))
		{
			if (_activeWeaponSlot == sourceSlotType)
			{
				_activeWeaponSlot = targetSlotType;
			}
			else if (_activeWeaponSlot == targetSlotType)
			{
				_activeWeaponSlot = sourceSlotType;
			}

			_activeWeaponSlot = ResolveActiveWeaponSlot();
		}

		RefreshEquipmentEffects();
		if (previousActiveWeaponSlot != _activeWeaponSlot)
		{
			EmitSignal(SignalName.ActiveWeaponSlotChanged, (int)_activeWeaponSlot);
		}

		EmitSignal(SignalName.EquipmentChanged);
		return true;
	}

	public bool CanEquip(EquipmentSlotType slotType, ItemInstance item)
	{
		return _slots.TryGetValue(slotType, out EquipmentSlotRecord? slot)
			&& CanEquip(slot, item, out _);
	}

	public ItemInstance? GetEquippedItem(EquipmentSlotType slotType)
	{
		return _slots.TryGetValue(slotType, out EquipmentSlotRecord? slot) ? slot.EquippedItem : null;
	}

	public ItemInstance? GetActiveWeaponItem()
	{
		return GetEquippedItem(ResolveActiveWeaponSlot());
	}

	public WeaponEquipmentDefinition? GetActiveWeaponDefinition()
	{
		return GetActiveWeaponItem()?.Definition.Equipment as WeaponEquipmentDefinition;
	}

	public FirearmWeaponDefinition? GetActiveFirearmDefinition()
	{
		return GetActiveWeaponItem()?.Definition.Equipment as FirearmWeaponDefinition;
	}

	public int GetActiveWeaponSkillSlotCount()
	{
		return GetActiveWeaponItem()?.GetWeaponSkillSlotCount() ?? 0;
	}

	public SkillDefinition? GetActiveWeaponSkillDefinition(int slotIndex)
	{
		return GetActiveWeaponItem()?.GetInstalledWeaponSkillDefinition(slotIndex);
	}

	public InstalledWeaponSkillState? GetActiveWeaponSkillState(int slotIndex)
	{
		return GetActiveWeaponItem()?.GetInstalledWeaponSkillState(slotIndex);
	}

	public bool TryInstallWeaponSkillOnActiveWeapon(
		WeaponSkillItemDefinition weaponSkillItem,
		ItemDefinition? sourceItemDefinition,
		ItemAcquisitionState sourceAcquisitionState,
		int sourceRemainingUses,
		out int installedSlotIndex)
	{
		installedSlotIndex = -1;
		return TryInstallWeaponSkillOnActiveWeaponCore(
			weaponSkillItem.Skill,
			weaponSkillItem,
			sourceItemDefinition,
			sourceAcquisitionState,
			sourceRemainingUses,
			preferredSlotIndex: null,
			replaceExisting: false,
			out installedSlotIndex);
	}

	public bool TryInstallWeaponSkillOnActiveWeaponSlot(
		WeaponSkillItemDefinition weaponSkillItem,
		ItemDefinition? sourceItemDefinition,
		ItemAcquisitionState sourceAcquisitionState,
		int sourceRemainingUses,
		int slotIndex,
		bool replaceExisting,
		out InstalledWeaponSkillState? replacedState)
	{
		replacedState = GetActiveWeaponSkillState(slotIndex)?.CreateCopy();
		if (!TryInstallWeaponSkillOnActiveWeaponCore(
			weaponSkillItem.Skill,
			weaponSkillItem,
			sourceItemDefinition,
			sourceAcquisitionState,
			sourceRemainingUses,
			slotIndex,
			replaceExisting,
			out int installedSlotIndex)
			|| installedSlotIndex != slotIndex)
		{
			replacedState = null;
			return false;
		}

		return true;
	}

	public bool TryInstallWeaponSkillOnEquippedWeaponSlot(
		EquipmentSlotType weaponSlotType,
		WeaponSkillItemDefinition weaponSkillItem,
		ItemDefinition? sourceItemDefinition,
		ItemAcquisitionState sourceAcquisitionState,
		int sourceRemainingUses,
		int slotIndex,
		bool replaceExisting,
		out InstalledWeaponSkillState? replacedState)
	{
		replacedState = null;
		ItemInstance? weaponItem = GetEquippedItem(weaponSlotType);
		WeaponEquipmentDefinition? weapon = weaponItem?.Definition.Equipment as WeaponEquipmentDefinition;
		if (!IsWeaponSlot(weaponSlotType)
			|| weaponItem == null
			|| weapon == null
			|| !weaponSkillItem.CanInstallOnWeapon(weapon))
		{
			return false;
		}

		if (!weaponItem.TryInstallWeaponSkillAtSlot(weaponSkillItem, sourceRemainingUses, sourceItemDefinition, sourceAcquisitionState, slotIndex, replaceExisting, out replacedState))
		{
			replacedState = null;
			return false;
		}

		EmitSignal(SignalName.EquipmentChanged);
		return true;
	}

	public bool TryClearEquippedWeaponSkillSlot(
		EquipmentSlotType weaponSlotType,
		int slotIndex,
		out InstalledWeaponSkillState? removedState)
	{
		removedState = null;
		ItemInstance? weaponItem = GetEquippedItem(weaponSlotType);
		if (!IsWeaponSlot(weaponSlotType) || weaponItem == null || !weaponItem.TryClearWeaponSkillSlot(slotIndex, out removedState))
		{
			return false;
		}

		EmitSignal(SignalName.EquipmentChanged);
		return true;
	}

	public bool TryRestoreEquippedWeaponSkillState(EquipmentSlotType weaponSlotType, InstalledWeaponSkillState state, bool replaceExisting)
	{
		ItemInstance? weaponItem = GetEquippedItem(weaponSlotType);
		if (!IsWeaponSlot(weaponSlotType) || weaponItem == null || !weaponItem.TryRestoreWeaponSkillState(state, replaceExisting))
		{
			return false;
		}

		EmitSignal(SignalName.EquipmentChanged);
		return true;
	}

	public bool TryClearActiveWeaponSkillSlot(int slotIndex, out InstalledWeaponSkillState? removedState)
	{
		removedState = null;
		ItemInstance? activeWeaponItem = GetActiveWeaponItem();
		if (activeWeaponItem == null || !activeWeaponItem.TryClearWeaponSkillSlot(slotIndex, out removedState))
		{
			return false;
		}

		EmitSignal(SignalName.EquipmentChanged);
		return true;
	}

	public IReadOnlyList<SkillDefinition> GetLearnedWeaponSkillsForActiveWeapon()
	{
		WeaponEquipmentDefinition? weapon = GetActiveWeaponDefinition();
		if (weapon == null || _weaponSkillKnowledge == null)
		{
			return [];
		}

		List<SkillDefinition> learnedSkills = [];
		foreach (SkillDefinition skill in _weaponSkillKnowledge.LearnedSkills)
		{
			if (SupportsLearnedWeaponSkill(weapon, skill))
			{
				learnedSkills.Add(skill);
			}
		}

		return learnedSkills;
	}

	public bool TryEquipLearnedWeaponSkillOnActiveWeapon(SkillDefinition skill, int slotIndex, bool replaceExisting)
	{
		return TryInstallWeaponSkillOnActiveWeaponCore(
			skill,
			weaponSkillItem: null,
			sourceItemDefinition: null,
			sourceAcquisitionState: ItemAcquisitionState.Base,
			sourceRemainingUses: 0,
			slotIndex,
			replaceExisting,
			out _);
	}

	public bool TryEquipFirstLearnedWeaponSkillOnActiveWeapon(int slotIndex)
	{
		foreach (SkillDefinition skill in GetLearnedWeaponSkillsForActiveWeapon())
		{
			if (TryEquipLearnedWeaponSkillOnActiveWeapon(skill, slotIndex, false))
			{
				return true;
			}
		}

		return false;
	}

	public bool NotifyActiveWeaponSkillCast(SkillDefinition skill)
	{
		ItemInstance? activeWeaponItem = GetActiveWeaponItem();
		InstalledWeaponSkillState? state = FindInstalledActiveWeaponSkillState(skill);
		bool learnedBefore = state?.PermanentlyUnlocked == true;
		if (activeWeaponItem == null || !activeWeaponItem.NotifyInstalledWeaponSkillCast(skill, out InstalledWeaponSkillState? depletedState))
		{
			return false;
		}

		TryPromoteLearnedWeaponSkill(state, learnedBefore);
		if (depletedState?.Skill != null)
		{
			TryPromoteLearnedWeaponSkill(depletedState, learnedBefore);
		}
		EmitSignal(SignalName.EquipmentChanged);
		return true;
	}

	public bool NotifyActiveWeaponSkillKill(SkillDefinition skill, bool bossKill)
	{
		ItemInstance? activeWeaponItem = GetActiveWeaponItem();
		InstalledWeaponSkillState? state = FindInstalledActiveWeaponSkillState(skill);
		bool learnedBefore = state?.PermanentlyUnlocked == true;
		if (activeWeaponItem == null || !activeWeaponItem.NotifyInstalledWeaponSkillKill(skill, bossKill))
		{
			return false;
		}

		TryPromoteLearnedWeaponSkill(state, learnedBefore);
		EmitSignal(SignalName.EquipmentChanged);
		return true;
	}

	public bool TrySetActiveWeaponSlot(EquipmentSlotType slotType)
	{
		if (!IsWeaponSlot(slotType) || !HasWeaponInSlot(slotType))
		{
			return false;
		}

		if (_activeWeaponSlot == slotType)
		{
			return true;
		}

		_activeWeaponSlot = slotType;
		RefreshEquipmentEffects();
		EmitSignal(SignalName.ActiveWeaponSlotChanged, (int)_activeWeaponSlot);
		EmitSignal(SignalName.EquipmentChanged);
		return true;
	}

	public bool TryToggleActiveWeaponSlot()
	{
		EquipmentSlotType otherSlot = ResolveActiveWeaponSlot() == EquipmentSlotType.WeaponSlot1
			? EquipmentSlotType.WeaponSlot2
			: EquipmentSlotType.WeaponSlot1;
		return TrySetActiveWeaponSlot(otherSlot);
	}

	public bool TryConsumeEquippedItem(EquipmentSlotType slotType, int amount)
	{
		if (amount <= 0 || !_slots.TryGetValue(slotType, out EquipmentSlotRecord? slot) || slot.EquippedItem == null)
		{
			return false;
		}

		if (amount >= slot.EquippedItem.StackCount)
		{
			slot.EquippedItem = null;
			RefreshEquipmentEffects();
			EmitSignal(SignalName.EquipmentChanged);
			return true;
		}

		if (!slot.EquippedItem.TrySetStackCount(slot.EquippedItem.StackCount - amount))
		{
			return false;
		}

		EmitSignal(SignalName.EquipmentChanged);
		return true;
	}

	public bool TryConsumeEquippedItemUse(EquipmentSlotType slotType)
	{
		if (!_slots.TryGetValue(slotType, out EquipmentSlotRecord? slot) || slot.EquippedItem == null)
		{
			return false;
		}

		if (!slot.EquippedItem.TryConsumeUse(out bool depleted))
		{
			return false;
		}

		if (depleted)
		{
			slot.EquippedItem = null;
			RefreshEquipmentEffects();
		}

		EmitSignal(SignalName.EquipmentChanged);
		return true;
	}

	public void ClearAllSlots()
	{
		bool changed = false;
		foreach (EquipmentSlotRecord slot in _slots.Values)
		{
			if (slot.EquippedItem == null)
			{
				continue;
			}

			slot.EquippedItem = null;
			changed = true;
		}

		if (!changed)
		{
			return;
		}

		RefreshEquipmentEffects();
		EmitSignal(SignalName.EquipmentChanged);
	}

	private static bool CanEquip(EquipmentSlotRecord slot, ItemInstance item, out EquipmentActionFailureReason failureReason)
	{
		if (!AllowsCategory(slot, item.Definition.Category))
		{
			failureReason = EquipmentActionFailureReason.CategoryNotAllowed;
			return false;
		}

		EquipmentDefinition? equipment = item.Definition.Equipment;
		if (equipment != null)
		{
			if (!equipment.AllowsSlot(slot.SlotType))
			{
				failureReason = EquipmentActionFailureReason.SlotNotAllowed;
				return false;
			}

			failureReason = EquipmentActionFailureReason.None;
			return true;
		}

		if (!slot.RequiresEquipmentDefinition && slot.AllowsUsableItems && item.Definition.Usable != null)
		{
			failureReason = EquipmentActionFailureReason.None;
			return true;
		}

		failureReason = EquipmentActionFailureReason.MissingEquipmentDefinition;
		return false;
	}

	private static bool AllowsCategory(EquipmentSlotRecord slot, ItemCategory category)
	{
		foreach (ItemCategory allowedCategory in slot.AllowedCategories)
		{
			if (allowedCategory == category)
			{
				return true;
			}
		}

		return false;
	}

	private void RefreshEquipmentEffects()
	{
		RefreshStatModifiers();
		RefreshWeaponMoveSet();
	}

	private void RefreshStatModifiers()
	{
		if (_statsComponent == null)
		{
			return;
		}

		foreach ((string key, StatType statType) in _appliedModifierKeys)
		{
			_statsComponent.RemoveStatModifier(key, statType);
		}

		_appliedModifierKeys.Clear();

		foreach ((EquipmentSlotType slotType, EquipmentSlotRecord slot) in _slots)
		{
			EquipmentDefinition? equipment = slot.EquippedItem?.Definition.Equipment;
			if (equipment == null)
			{
				continue;
			}

			Godot.Collections.Array<EquipmentStatModifierDefinition> resolvedModifiers = equipment.GetResolvedStatModifiers();
			for (int modifierIndex = 0; modifierIndex < resolvedModifiers.Count; modifierIndex++)
			{
				EquipmentStatModifierDefinition modifier = resolvedModifiers[modifierIndex];
				string key = $"equipment:{slotType}:{modifierIndex}";
				_statsComponent.SetStatModifier(key, modifier.StatType, modifier.Flat, modifier.Multiplier);
				_appliedModifierKeys.Add((key, modifier.StatType));
			}

			ApplyStructuredEquipmentModifiers(slotType, equipment);
		}
	}

	private void ApplyStructuredEquipmentModifiers(EquipmentSlotType slotType, EquipmentDefinition equipment)
	{
		if (_statsComponent == null)
		{
			return;
		}

		if (equipment is WeaponEquipmentDefinition weapon)
		{
			if (slotType != ResolveActiveWeaponSlot())
			{
				return;
			}

			ApplyStructuredModifier(slotType, "weapon_pt", StatType.WeaponPenetrationTier, weapon.GetBasePenetrationTier());
		}

		if (equipment is ArmorEquipmentDefinition armor)
		{
			ApplyStructuredModifier(slotType, "armor_tier", StatType.ArmorTier, armor.ArmorTier);
			ApplyStructuredModifier(slotType, "armor_ap", StatType.ArmorPointMax, armor.MaxArmorPoint);
			ApplyStructuredModifier(slotType, "armor_absorb", StatType.ArmorAbsorbRate, armor.BaseAbsorbRate);
		}

		if (equipment is GloveEquipmentDefinition gloves)
		{
			ApplyStructuredModifier(slotType, "glove_attack", StatType.AttackPower, gloves.AttackPower);
		}
	}

	private void ApplyStructuredModifier(EquipmentSlotType slotType, string suffix, StatType statType, float flat, float multiplier = 1.0f)
	{
		if (_statsComponent == null)
		{
			return;
		}

		string key = $"equipment:{slotType}:{suffix}";
		_statsComponent.SetStatModifier(key, statType, flat, multiplier);
		_appliedModifierKeys.Add((key, statType));
	}

	private void RefreshWeaponMoveSet()
	{
		if (_skillLoadout == null)
		{
			return;
		}

		WeaponEquipmentDefinition? weapon = GetActiveWeaponDefinition();
		_skillLoadout.ApplyWeaponMoveSet(weapon?.MoveSet);
	}

	private EquipmentSlotType ResolveActiveWeaponSlot()
	{
		if (HasWeaponInSlot(_activeWeaponSlot))
		{
			return _activeWeaponSlot;
		}

		if (HasWeaponInSlot(EquipmentSlotType.WeaponSlot1))
		{
			return EquipmentSlotType.WeaponSlot1;
		}

		if (HasWeaponInSlot(EquipmentSlotType.WeaponSlot2))
		{
			return EquipmentSlotType.WeaponSlot2;
		}

		return _activeWeaponSlot;
	}

	private bool HasWeaponInSlot(EquipmentSlotType slotType)
	{
		return IsWeaponSlot(slotType)
			&& GetEquippedItem(slotType)?.Definition.Equipment is WeaponEquipmentDefinition;
	}

	private static bool IsWeaponSlot(EquipmentSlotType slotType)
	{
		return slotType == EquipmentSlotType.WeaponSlot1 || slotType == EquipmentSlotType.WeaponSlot2;
	}

	private static bool SupportsLearnedWeaponSkill(WeaponEquipmentDefinition weapon, SkillDefinition skill)
	{
		return skill.LoadoutCategory == SkillLoadoutCategory.Weapon
			&& weapon.SupportsWeaponSkill(skill)
			&& skill.SupportsWeapon(weapon);
	}

	private bool TryInstallWeaponSkillOnActiveWeaponCore(
		SkillDefinition? skill,
		WeaponSkillItemDefinition? weaponSkillItem,
		ItemDefinition? sourceItemDefinition,
		ItemAcquisitionState sourceAcquisitionState,
		int sourceRemainingUses,
		int? preferredSlotIndex,
		bool replaceExisting,
		out int installedSlotIndex)
	{
		installedSlotIndex = -1;
		ItemInstance? activeWeaponItem = GetActiveWeaponItem();
		WeaponEquipmentDefinition? weapon = GetActiveWeaponDefinition();
		if (activeWeaponItem == null || weapon == null || skill == null)
		{
			return false;
		}

		bool useLearnedInstall = _weaponSkillKnowledge?.HasLearnedSkill(skill) == true;
		if (useLearnedInstall)
		{
			if (!SupportsLearnedWeaponSkill(weapon, skill))
			{
				return false;
			}

			if (preferredSlotIndex.HasValue)
			{
				if (!activeWeaponItem.TryEquipLearnedWeaponSkill(skill, preferredSlotIndex.Value, replaceExisting))
				{
					return false;
				}

				installedSlotIndex = preferredSlotIndex.Value;
				EmitSignal(SignalName.EquipmentChanged);
				return true;
			}

			for (int index = 0; index < activeWeaponItem.GetWeaponSkillSlotCount(); index++)
			{
				if (!activeWeaponItem.GetInstalledWeaponSkillState(index)?.IsEmpty ?? true)
				{
					continue;
				}

				if (!activeWeaponItem.TryEquipLearnedWeaponSkill(skill, index, false))
				{
					continue;
				}

				installedSlotIndex = index;
				EmitSignal(SignalName.EquipmentChanged);
				return true;
			}

			return false;
		}

		if (weaponSkillItem == null || !weaponSkillItem.CanInstallOnWeapon(weapon))
		{
			return false;
		}

		if (preferredSlotIndex.HasValue)
		{
			if (!activeWeaponItem.TryInstallWeaponSkillAtSlot(weaponSkillItem, sourceRemainingUses, sourceItemDefinition, sourceAcquisitionState, preferredSlotIndex.Value, replaceExisting, out _))
			{
				return false;
			}

			installedSlotIndex = preferredSlotIndex.Value;
			EmitSignal(SignalName.EquipmentChanged);
			return true;
		}

		if (!activeWeaponItem.TryInstallWeaponSkill(weaponSkillItem, sourceRemainingUses, sourceItemDefinition, sourceAcquisitionState, out installedSlotIndex))
		{
			return false;
		}

		EmitSignal(SignalName.EquipmentChanged);
		return true;
	}

	private InstalledWeaponSkillState? FindInstalledActiveWeaponSkillState(SkillDefinition skill)
	{
		ItemInstance? activeWeaponItem = GetActiveWeaponItem();
		if (activeWeaponItem == null)
		{
			return null;
		}

		for (int slotIndex = 0; slotIndex < activeWeaponItem.GetWeaponSkillSlotCount(); slotIndex++)
		{
			InstalledWeaponSkillState? state = activeWeaponItem.GetInstalledWeaponSkillState(slotIndex);
			if (state?.MatchesSkill(skill) == true)
			{
				return state;
			}
		}

		return null;
	}

	private void TryPromoteLearnedWeaponSkill(InstalledWeaponSkillState? state, bool learnedBefore)
	{
		if (learnedBefore || state?.Skill == null || state.PermanentlyUnlocked == false)
		{
			return;
		}

		_weaponSkillKnowledge?.LearnSkill(state.Skill);
	}

	private void EnsureDefaultSlots()
	{
		if (_slots.Count > 0)
		{
			return;
		}

		AddSlot(EquipmentSlotType.WeaponSlot1, "Weapon Slot 1", true, false, string.Empty, ItemCategory.Weapon);
		AddSlot(EquipmentSlotType.WeaponSlot2, "Weapon Slot 2", true, false, string.Empty, ItemCategory.Weapon);
		AddSlot(EquipmentSlotType.Gloves, "Gloves", true, false, string.Empty, ItemCategory.Gloves);
		AddSlot(EquipmentSlotType.Armor, "Armor", true, false, string.Empty, ItemCategory.Armor);
		AddSlot(EquipmentSlotType.Shoes, "Shoes", true, false, string.Empty, ItemCategory.Armor);
		AddSlot(EquipmentSlotType.Item1, "Quick Item 1", false, true, "1", ItemCategory.KeyItem, ItemCategory.Consumable, ItemCategory.Medical);
		AddSlot(EquipmentSlotType.Item2, "Quick Item 2", false, true, "2", ItemCategory.KeyItem, ItemCategory.Consumable, ItemCategory.Medical);
		AddSlot(EquipmentSlotType.Item3, "Quick Item 3", false, true, "3", ItemCategory.KeyItem, ItemCategory.Consumable, ItemCategory.Medical);
	}

	private void AddSlot(
		EquipmentSlotType slotType,
		string displayName,
		bool requiresEquipmentDefinition,
		bool allowsUsableItems,
		string shortcutHint,
		params ItemCategory[] allowedCategories)
	{
		_slots[slotType] = new EquipmentSlotRecord
		{
			SlotType = slotType,
			DisplayName = displayName,
			AllowedCategories = allowedCategories,
			RequiresEquipmentDefinition = requiresEquipmentDefinition,
			AllowsUsableItems = allowsUsableItems,
			ShortcutHint = shortcutHint,
		};
	}
}
