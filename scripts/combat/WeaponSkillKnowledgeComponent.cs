using Godot;
using System.Collections.Generic;

namespace BattleHarvesterStudy.Combat;

public partial class WeaponSkillKnowledgeComponent : Node
{
	private readonly Dictionary<string, SkillDefinition> _learnedSkills = [];

	public IReadOnlyCollection<SkillDefinition> LearnedSkills => _learnedSkills.Values;

	public bool LearnSkill(SkillDefinition? skill)
	{
		if (skill == null || string.IsNullOrWhiteSpace(skill.SkillId))
		{
			return false;
		}

		if (_learnedSkills.ContainsKey(skill.SkillId))
		{
			return false;
		}

		_learnedSkills[skill.SkillId] = skill;
		return true;
	}

	public bool HasLearnedSkill(SkillDefinition? skill)
	{
		return skill != null && HasLearnedSkill(skill.SkillId);
	}

	public bool HasLearnedSkill(string? skillId)
	{
		return !string.IsNullOrWhiteSpace(skillId) && _learnedSkills.ContainsKey(skillId);
	}

	public void ClearLearnedSkills()
	{
		_learnedSkills.Clear();
	}

	public void RestoreLearnedSkills(IEnumerable<SkillDefinition> skills)
	{
		_learnedSkills.Clear();
		foreach (SkillDefinition skill in skills)
		{
			LearnSkill(skill);
		}
	}
}
