using Godot;
using System.Collections.Generic;
using BattleHarvesterStudy.Combat.Firearms;

namespace BattleHarvesterStudy.Presentation;

public partial class EquipmentPanelPresenter : Node
{
	[Signal]
	public delegate void SlotPressedEventHandler(int slotType, int buttonIndex);

	[Signal]
	public delegate void SlotDoublePressedEventHandler(int slotType);

	[Export] public NodePath UiContextPath { get; set; } = new("../PlayerUiContext");
	[Export] public NodePath HeaderLabelPath { get; set; } = new("../../InventoryUi/PlayerInventoryWindow/Margin/VBox/ContentRow/EquipmentColumn/EquipmentPanel/Margin/VBox/Header");
	[Export] public NodePath SummaryLabelPath { get; set; } = new("../../InventoryUi/PlayerInventoryWindow/Margin/VBox/ContentRow/EquipmentColumn/EquipmentPanel/Margin/VBox/Summary");
	[Export] public NodePath WeaponSlot1Path { get; set; } = new("../../InventoryUi/PlayerInventoryWindow/Margin/VBox/ContentRow/EquipmentColumn/EquipmentPanel/Margin/VBox/Slots/WeaponSlot1");
	[Export] public NodePath WeaponSlot2Path { get; set; } = new("../../InventoryUi/PlayerInventoryWindow/Margin/VBox/ContentRow/EquipmentColumn/EquipmentPanel/Margin/VBox/Slots/WeaponSlot2");
	[Export] public NodePath GlovesSlotPath { get; set; } = new("../../InventoryUi/PlayerInventoryWindow/Margin/VBox/ContentRow/EquipmentColumn/EquipmentPanel/Margin/VBox/Slots/GlovesSlot");
	[Export] public NodePath ArmorSlotPath { get; set; } = new("../../InventoryUi/PlayerInventoryWindow/Margin/VBox/ContentRow/EquipmentColumn/EquipmentPanel/Margin/VBox/Slots/ArmorSlot");
	[Export] public NodePath ShoesSlotPath { get; set; } = new("../../InventoryUi/PlayerInventoryWindow/Margin/VBox/ContentRow/EquipmentColumn/EquipmentPanel/Margin/VBox/Slots/ChestSlot");
	[Export] public NodePath Item1SlotPath { get; set; } = new("../../InventoryUi/PlayerInventoryWindow/Margin/VBox/ContentRow/EquipmentColumn/EquipmentPanel/Margin/VBox/Slots/LegsSlot");
	[Export] public NodePath Item2SlotPath { get; set; } = new("../../InventoryUi/PlayerInventoryWindow/Margin/VBox/ContentRow/EquipmentColumn/EquipmentPanel/Margin/VBox/Slots/HandsSlot");
	[Export] public NodePath Item3SlotPath { get; set; } = new("../../InventoryUi/PlayerInventoryWindow/Margin/VBox/ContentRow/EquipmentColumn/EquipmentPanel/Margin/VBox/Slots/ShoesSlot");
	[Export] public NodePath UnusedSlot1Path { get; set; } = new("../../InventoryUi/PlayerInventoryWindow/Margin/VBox/ContentRow/EquipmentColumn/EquipmentPanel/Margin/VBox/Slots/Item1Slot");
	[Export] public NodePath UnusedSlot2Path { get; set; } = new("../../InventoryUi/PlayerInventoryWindow/Margin/VBox/ContentRow/EquipmentColumn/EquipmentPanel/Margin/VBox/Slots/Accessory2Slot");

	private readonly Dictionary<EquipmentSlotType, Button> _slotButtons = [];
	private Label? _headerLabel;
	private Label? _summaryLabel;
	private EquipmentComponent? _equipmentComponent;
	private PlayerUiContext? _uiContext;

	public override void _Ready()
	{
		_uiContext = GetNodeOrNull<PlayerUiContext>(UiContextPath);
		_headerLabel = GetNodeOrNull<Label>(HeaderLabelPath);
		_summaryLabel = GetNodeOrNull<Label>(SummaryLabelPath);
		_equipmentComponent = _uiContext?.Equipment;

		BindSlot(EquipmentSlotType.WeaponSlot1, WeaponSlot1Path);
		BindSlot(EquipmentSlotType.WeaponSlot2, WeaponSlot2Path);
		BindSlot(EquipmentSlotType.Gloves, GlovesSlotPath);
		BindSlot(EquipmentSlotType.Armor, ArmorSlotPath);
		BindSlot(EquipmentSlotType.Shoes, ShoesSlotPath);
		BindSlot(EquipmentSlotType.Item1, Item1SlotPath);
		BindSlot(EquipmentSlotType.Item2, Item2SlotPath);
		BindSlot(EquipmentSlotType.Item3, Item3SlotPath);

		HideUnusedSlotButton(UnusedSlot1Path);
		HideUnusedSlotButton(UnusedSlot2Path);
	}

	public void Present()
	{
		if (_headerLabel != null)
		{
			_headerLabel.Text = UiText.Resolve(UiTextKeys.Inventory.HeaderEquipment);
		}

		if (_equipmentComponent == null)
		{
			return;
		}

		int equippedCount = 0;
		foreach ((EquipmentSlotType slotType, Button button) in _slotButtons)
		{
			if (!_equipmentComponent.Slots.TryGetValue(slotType, out EquipmentSlotRecord? slot))
			{
				continue;
			}

			if (slot.EquippedItem != null)
			{
				equippedCount++;
			}

			button.Text = BuildSlotButtonText(slot, _equipmentComponent.ActiveWeaponSlot);
		}

		if (_summaryLabel != null)
		{
			_summaryLabel.Text = UiText.Resolve(
				UiTextKeys.Inventory.EquipmentSummary,
				("equipped_count", equippedCount),
				("slot_count", _slotButtons.Count),
				("open_hint", UiText.Resolve(UiTextKeys.Inventory.HintInspectSlot)),
				("drag_hint", UiText.Resolve(UiTextKeys.Inventory.HintDragToEquip)));
		}
	}

	public bool TryGetSlotFromGlobalPosition(Vector2 globalPosition, out EquipmentSlotType slotType)
	{
		foreach ((EquipmentSlotType currentSlotType, Button button) in _slotButtons)
		{
			if (!button.Visible)
			{
				continue;
			}

			if (button.GetGlobalRect().HasPoint(globalPosition))
			{
				slotType = currentSlotType;
				return true;
			}
		}

		slotType = EquipmentSlotType.None;
		return false;
	}

	private static string BuildSlotButtonText(EquipmentSlotRecord slot, EquipmentSlotType activeWeaponSlot)
	{
		string displayName = EquipmentTextFormatter.GetSlotDisplayName(slot.SlotType);
		if (slot.SlotType == activeWeaponSlot
			&& (slot.SlotType == EquipmentSlotType.WeaponSlot1 || slot.SlotType == EquipmentSlotType.WeaponSlot2))
		{
			displayName = $"> {displayName}";
		}

		string slotName = string.IsNullOrWhiteSpace(slot.ShortcutHint)
			? displayName
			: $"[{slot.ShortcutHint}] {displayName}";

		if (slot.EquippedItem == null)
		{
			return $"{slotName}\n{EquipmentTextFormatter.GetUnequippedText()}";
		}

		bool chinese = UiText.CurrentLocale == UiText.DefaultLocale;
		string itemName = ContentTextFormatter.GetItemDisplayName(slot.EquippedItem.Definition);
		if (slot.EquippedItem.HasLimitedUses)
		{
			itemName = $"{itemName} ({slot.EquippedItem.RemainingUses})";
		}
		else if (slot.EquippedItem.StackCount > 1)
		{
			itemName = $"{itemName} x{slot.EquippedItem.StackCount}";
		}

		if (slot.EquippedItem.Definition.Equipment is FirearmWeaponDefinition firearm)
		{
			FirearmResolvedStats resolved = FirearmStatResolver.Resolve(firearm, slot.EquippedItem);
			itemName = $"{itemName}  {(chinese ? "弹匣" : "MAG")} {slot.EquippedItem.CurrentMagazineAmmo}/{resolved.MagazineCapacity}";
		}
		else if (slot.EquippedItem.HasDurability)
		{
			itemName = $"{itemName}  {slot.EquippedItem.CurrentDurability:0}/{slot.EquippedItem.CurrentMaxDurability:0}";
		}
		else if (slot.EquippedItem.HasArmorPoint)
		{
			itemName = $"{itemName}  AP {slot.EquippedItem.CurrentArmorPoint:0}/{slot.EquippedItem.CurrentMaxArmorPoint:0}";
		}

		return $"{slotName}\n{itemName}";
	}

	private void BindSlot(EquipmentSlotType slotType, NodePath path)
	{
		Button? button = GetNodeOrNull<Button>(path);
		if (button == null)
		{
			return;
		}

		_slotButtons[slotType] = button;
		button.GuiInput += @event =>
		{
			if (@event is not InputEventMouseButton mouseButton || !mouseButton.Pressed)
			{
				return;
			}

			if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.DoubleClick)
			{
				EmitSignal(SignalName.SlotDoublePressed, (int)slotType);
				return;
			}

			EmitSignal(SignalName.SlotPressed, (int)slotType, (int)mouseButton.ButtonIndex);
		};
	}

	private void HideUnusedSlotButton(NodePath path)
	{
		Button? button = GetNodeOrNull<Button>(path);
		if (button == null)
		{
			return;
		}

		button.Visible = false;
		button.MouseFilter = Control.MouseFilterEnum.Ignore;
	}
}
