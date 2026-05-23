using Godot;

namespace BattleHarvesterStudy.Presentation;

[GlobalClass]
public partial class StatusPresentationDefinition : Resource
{
	[Export]
	public string StatusId { get; set; } = "status_default";

	[Export]
	public string AttachVfxKey { get; set; } = string.Empty;

	[Export]
	public string ApplyVfxKey { get; set; } = string.Empty;

	[Export]
	public string ExpireVfxKey { get; set; } = string.Empty;

	[Export]
	public string IconPath { get; set; } = string.Empty;
}
