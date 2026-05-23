using Godot;

namespace BattleHarvesterStudy.Combat;

public readonly struct HitResult
{
	public Hurtbox Target { get; }
	public Node3D Caster { get; }
	public SkillDefinition Skill { get; }
	public Vector3 HitDirection { get; }
	public int HitIndex { get; }

	public HitResult(Hurtbox target, Node3D caster, SkillDefinition skill, Vector3 hitDirection, int hitIndex)
	{
		Target = target;
		Caster = caster;
		Skill = skill;
		HitDirection = hitDirection;
		HitIndex = hitIndex;
	}
}
