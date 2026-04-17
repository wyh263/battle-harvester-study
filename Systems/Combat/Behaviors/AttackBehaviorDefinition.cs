using Godot;

namespace BattleHarvesterStudy;

[GlobalClass]
public abstract partial class AttackBehaviorDefinition : Resource
{
	public abstract void Begin(Hitbox? hitbox, SkillExecutionContext context);
	public abstract void End(Hitbox? hitbox, SkillExecutionContext context);
}
