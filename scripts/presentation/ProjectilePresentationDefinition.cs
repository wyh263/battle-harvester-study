using Godot;

namespace BattleHarvesterStudy.Presentation;

[GlobalClass]
public partial class ProjectilePresentationDefinition : Resource
{
	[Export]
	public string PresentationId { get; set; } = "projectile_default";

	[Export]
	public string VisualScenePath { get; set; } = string.Empty;

	[Export]
	public string SpawnVfxKey { get; set; } = string.Empty;

	[Export]
	public string LoopVfxKey { get; set; } = string.Empty;

	[Export]
	public string HitVfxKey { get; set; } = string.Empty;

	[Export]
	public string DespawnVfxKey { get; set; } = string.Empty;

	[Export]
	public string AudioKey { get; set; } = string.Empty;
}
