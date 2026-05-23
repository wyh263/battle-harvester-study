using Godot;

namespace BattleHarvesterStudy.Combat;

[GlobalClass]
public partial class LaunchEffectDefinition : EffectDefinition
{
	[Export]
	public float Height { get; set; } = 1.2f;

	[Export]
	public float ForwardDistance { get; set; } = 1.0f;

	[Export]
	public float DurationSeconds { get; set; } = 0.35f;

	public override void Apply(HitResult hitResult)
	{
		Node3D? target = hitResult.Target.GetOwner<Node3D>();
		LaunchMotionComponent? launchMotion = target?.GetNodeOrNull<LaunchMotionComponent>("Components/LaunchMotion");
		launchMotion?.ApplyLaunch(hitResult.HitDirection, Height, ForwardDistance, DurationSeconds);
	}
}
