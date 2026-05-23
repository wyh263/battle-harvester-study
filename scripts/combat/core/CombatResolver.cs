using Godot;

namespace BattleHarvesterStudy.Combat;

public partial class CombatResolver : Node
{
	public void ResolveHit(HitResult hitResult)
	{
		SkillChainTracker? chainTracker = hitResult.Caster.GetNodeOrNull<SkillChainTracker>("Components/SkillChainTracker");
		chainTracker?.RecordHit(hitResult);

		CombatPresentationEvents.PublishHitResolved(hitResult);

		foreach (EffectDefinition effect in hitResult.Skill.Effects)
		{
			effect.Apply(hitResult);
		}
	}
}
