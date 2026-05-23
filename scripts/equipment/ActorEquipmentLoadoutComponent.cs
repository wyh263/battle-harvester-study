using Godot;
using BattleHarvesterStudy.Actors;
using BattleHarvesterStudy.Items;

namespace BattleHarvesterStudy.Equipment;

public partial class ActorEquipmentLoadoutComponent : Node
{
	[Export]
	public ItemDefinition? WeaponSlot1Definition { get; set; }

	[Export]
	public ItemDefinition? WeaponSlot2Definition { get; set; }

	[Export]
	public ItemDefinition? GlovesDefinition { get; set; }

	[Export]
	public ItemDefinition? ArmorDefinition { get; set; }

	[Export]
	public ItemDefinition? ShoesDefinition { get; set; }

	[Export]
	public string FacingLabelOverride { get; set; } = string.Empty;

	[Export]
	public NodePath EquipmentPath { get; set; } = "../Equipment";

	public override void _Ready()
	{
		EquipmentComponent? equipment = GetNodeOrNull<EquipmentComponent>(EquipmentPath);
		if (equipment == null)
		{
			GD.PushWarning($"{nameof(ActorEquipmentLoadoutComponent)} on {GetPath()} could not find equipment component.");
			return;
		}

		TryEquip(equipment, EquipmentSlotType.WeaponSlot1, WeaponSlot1Definition);
		TryEquip(equipment, EquipmentSlotType.WeaponSlot2, WeaponSlot2Definition);
		TryEquip(equipment, EquipmentSlotType.Gloves, GlovesDefinition);
		TryEquip(equipment, EquipmentSlotType.Armor, ArmorDefinition);
		TryEquip(equipment, EquipmentSlotType.Shoes, ShoesDefinition);

		if (!string.IsNullOrWhiteSpace(FacingLabelOverride) && GetOwner<Node>() is Dummy dummy)
		{
			dummy.FacingLabel = FacingLabelOverride;
		}
	}

	private static void TryEquip(EquipmentComponent equipment, EquipmentSlotType slotType, ItemDefinition? definition)
	{
		if (definition == null)
		{
			return;
		}

		ItemInstance instance = new(definition);
		if (!equipment.TryEquip(slotType, instance, out _, out EquipmentActionFailureReason failureReason))
		{
			GD.PushWarning($"Failed to auto-equip {definition.ItemId} to {slotType}: {failureReason}");
		}
	}
}
