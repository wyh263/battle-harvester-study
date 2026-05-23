using Godot;

namespace BattleHarvesterStudy.Combat;

public sealed class SkillExecutionContext
{
	public Node3D Caster { get; }
	public SkillDefinition Skill { get; }
	public Vector3 OriginPosition { get; }
	public Vector3 FacingDirection { get; }

	public SkillExecutionContext(Node3D caster, SkillDefinition skill, Vector3 originPosition, Vector3 facingDirection)
	{
		Caster = caster;
		Skill = skill;
		OriginPosition = originPosition;
		FacingDirection = facingDirection;
	}
}
