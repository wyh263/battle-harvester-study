using Godot;

namespace BattleHarvesterStudy.Combat;

[GlobalClass]
public partial class ShapeAttackBehaviorDefinition : AttackBehaviorDefinition
{
	[Export]
	public ShapeDefinition? Shape { get; set; }

	public override void Begin(Hitbox? hitbox, SkillExecutionContext context)
	{
		if (hitbox == null)
		{
			return;
		}

		hitbox.ActivateShape(context, Shape ?? ShapeDefinition.CreateDefault());
	}

	public override void End(Hitbox? hitbox, SkillExecutionContext context)
	{
		hitbox?.DeactivateShape();
	}
}
