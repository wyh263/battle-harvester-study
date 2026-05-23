using Godot;
using Godot.Collections;

namespace BattleHarvesterStudy.Equipment;

[GlobalClass]
public partial class EquipmentDefinition : Resource
{
	[Export]
	public EquipmentArchetypeDefinition? Archetype { get; set; }

	[Export]
	public string DisplayVariant { get; set; } = "Standard";

	[Export]
	public EquipmentKind Kind { get; set; } = EquipmentKind.Generic;

	[Export]
	public EquipmentRarity Rarity { get; set; } = EquipmentRarity.Common;

	[Export(PropertyHint.Range, "1,20,1")]
	public int Tier { get; set; } = 1;

	[Export(PropertyHint.Range, "1,100,1")]
	public int ItemLevel { get; set; } = 1;

	[Export]
	public Array<EquipmentSlotType> AllowedSlots { get; set; } = [];

	[Export]
	public EquipmentGripType GripType { get; set; } = EquipmentGripType.OneHanded;

	[Export]
	public Array<EquipmentStatModifierDefinition> BaseModifiers { get; set; } = [];

	[Export]
	public Array<EquipmentAffixDefinition> Affixes { get; set; } = [];

	public virtual Array<EquipmentStatModifierDefinition> GetResolvedStatModifiers()
	{
		Array<EquipmentStatModifierDefinition> resolved = [];
		foreach (EquipmentStatModifierDefinition modifier in BaseModifiers)
		{
			resolved.Add(modifier);
		}

		foreach (EquipmentAffixDefinition affix in Affixes)
		{
			foreach (EquipmentStatModifierDefinition modifier in affix.Modifiers)
			{
				resolved.Add(modifier);
			}
		}

		return resolved;
	}

	public bool AllowsSlot(EquipmentSlotType slotType)
	{
		if (AllowedSlots.Count > 0)
		{
			foreach (EquipmentSlotType allowedSlot in AllowedSlots)
			{
				if (allowedSlot == slotType)
				{
					return true;
				}
			}

			return false;
		}

		return Archetype?.AllowsSlot(slotType) ?? false;
	}
}
