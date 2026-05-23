using Godot;

namespace BattleHarvesterStudy.Combat;

[GlobalClass]
public abstract partial class EffectDefinition : Resource
{
	public abstract void Apply(HitResult hitResult);
}
