using Godot;

namespace BattleHarvesterStudy.Presentation;

[GlobalClass]
public partial class SkillPresentationDefinition : Resource
{
	[Export]
	public string PresentationId { get; set; } = "skill_default";

	[Export]
	public string CastAnimation { get; set; } = string.Empty;

	[Export]
	public string StartupVfxKey { get; set; } = string.Empty;

	[Export]
	public string ActiveVfxKey { get; set; } = string.Empty;

	[Export]
	public string HitVfxKey { get; set; } = string.Empty;

	[Export]
	public string EndVfxKey { get; set; } = string.Empty;

	[Export]
	public string CastAudioKey { get; set; } = string.Empty;

	[Export]
	public string UiIconPath { get; set; } = string.Empty;

	[Export]
	public ProjectilePresentationDefinition? ProjectilePresentation { get; set; }
}
