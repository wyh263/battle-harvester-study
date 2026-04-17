using Godot;

namespace BattleHarvesterStudy;

public enum HitboxShapeType
{
	Box,
	Cylinder,
	Sphere
}

[GlobalClass]
public partial class ShapeDefinition : Resource
{
	[Export]
	public HitboxShapeType ShapeType { get; set; } = HitboxShapeType.Box;

	[Export]
	public Vector3 Size { get; set; } = new(1.1f, 0.9f, 1.0f);

	[Export]
	public Vector3 Offset { get; set; } = new(0.0f, 0.45f, -1.0f);

	[Export]
	public Vector3 RotationDegrees { get; set; } = Vector3.Zero;

	[Export]
	public int MaxHits { get; set; } = 1;

	[Export]
	public bool AllowRepeatHitOnSameTarget { get; set; } = false;

	[Export]
	public float RepeatHitInterval { get; set; } = 0.0f;

	[Export]
	public Color DebugColor { get; set; } = new(1.0f, 0.4f, 0.2f, 0.2f);

	[Export]
	public bool ShowDebugPreview { get; set; } = true;

	public static ShapeDefinition CreateDefault()
	{
		return new ShapeDefinition();
	}
}
