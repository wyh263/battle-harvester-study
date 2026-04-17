using Godot;

namespace BattleHarvesterStudy;

[GlobalClass]
public abstract partial class EffectDefinition : Resource
{
	public abstract void Apply(HitResult hitResult);
}
