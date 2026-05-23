using Godot;
using Godot.Collections;

namespace BattleHarvesterStudy.Combat;

[GlobalClass]
public partial class PlayerGeneralSkillLoadoutComponent : Node
{
	[Export(PropertyHint.Range, "0,8,1")]
	public int UnlockedSlotCount { get; set; } = 2;

	[Export]
	public Array<SkillDefinition> EquippedSkills { get; set; } = [];

	public int GetUnlockedSlotCount()
	{
		return Mathf.Max(0, UnlockedSlotCount);
	}

	public SkillDefinition? GetSkill(int slotIndex)
	{
		if (slotIndex < 0 || slotIndex >= GetUnlockedSlotCount() || slotIndex >= EquippedSkills.Count)
		{
			return null;
		}

		return EquippedSkills[slotIndex];
	}

	public bool TryEquipSkill(int slotIndex, SkillDefinition skill)
	{
		if (slotIndex < 0 || slotIndex >= GetUnlockedSlotCount() || skill.LoadoutCategory != SkillLoadoutCategory.General)
		{
			return false;
		}

		while (EquippedSkills.Count <= slotIndex)
		{
			EquippedSkills.Add(null);
		}

		EquippedSkills[slotIndex] = skill;
		return true;
	}
}
