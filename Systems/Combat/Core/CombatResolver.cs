using Godot;

namespace BattleHarvesterStudy;

public partial class CombatResolver : Node
{
	public void ResolveHit(HitResult hitResult)
	{
		foreach (EffectDefinition effect in hitResult.Skill.Effects)
		{
			effect.Apply(hitResult);
		}
	}
}
