using Godot;
using System.Collections.Generic;
using System;

namespace BattleHarvesterStudy;

public partial class Hitbox : Area3D
{
	[Export]
	public bool ShowDebugPreview { get; set; } = true;

	private Node3D? _owner;
	private CollisionShape3D? _collisionShape;
	private MeshInstance3D? _debugPreview;
	private CombatResolver? _combatResolver;
	private ShapeDefinition? _defaultShape;
	private ShapeDefinition? _activeShape;
	private SkillExecutionContext? _activeContext;
	private Func<Hurtbox, bool>? _targetFilter;
	private readonly Dictionary<Hurtbox, int> _targetHitCounts = new();
	private readonly Dictionary<Hurtbox, double> _targetNextHitTimes = new();
	private double _activeElapsedSeconds;
	private bool _isActive;

	public override void _Ready()
	{
		_owner = GetOwner<Node3D>();
		_collisionShape = GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
		_debugPreview = GetNodeOrNull<MeshInstance3D>("DebugPreview");
		_combatResolver = _owner?.GetNodeOrNull<CombatResolver>("Components/CombatResolver");
		_defaultShape = CaptureCurrentShape();
		SetPhysicsProcess(false);
		Monitoring = false;
		SetDebugPreviewVisible(false);
		AreaEntered += OnAreaEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_isActive || _activeShape == null)
		{
			return;
		}

		_activeElapsedSeconds += delta;

		foreach (Area3D area in GetOverlappingAreas())
		{
			if (area is Hurtbox hurtbox)
			{
				TryApplyHit(hurtbox);
			}
		}
	}

	public void ActivateShape(SkillExecutionContext context, ShapeDefinition? shape, Func<Hurtbox, bool>? targetFilter = null)
	{
		_activeContext = context;
		_activeShape = shape ?? _defaultShape ?? ShapeDefinition.CreateDefault();
		_targetFilter = targetFilter;
		ApplyShapeDefinition(_activeShape);
		_targetHitCounts.Clear();
		_targetNextHitTimes.Clear();
		_activeElapsedSeconds = 0.0;
		_isActive = true;
		SetPhysicsProcess(true);
		Monitoring = true;
		SetDebugPreviewVisible(_activeShape.ShowDebugPreview);
	}

	public void DeactivateShape()
	{
		_isActive = false;
		Monitoring = false;
		_activeContext = null;
		_activeShape = null;
		_targetFilter = null;
		_targetHitCounts.Clear();
		_targetNextHitTimes.Clear();
		_activeElapsedSeconds = 0.0;
		SetPhysicsProcess(false);
		SetDebugPreviewVisible(false);
	}

	private void OnAreaEntered(Area3D area)
	{
		if (!_isActive)
		{
			return;
		}

		if (area is not Hurtbox hurtbox)
		{
			return;
		}

		TryApplyHit(hurtbox);
	}

	private void SetDebugPreviewVisible(bool visible)
	{
		if (_debugPreview == null)
		{
			return;
		}

		_debugPreview.Visible = visible;
	}

	private ShapeDefinition CaptureCurrentShape()
	{
		ShapeDefinition definition = ShapeDefinition.CreateDefault();

		if (_collisionShape != null)
		{
			definition.Offset = _collisionShape.Position;
			definition.RotationDegrees = _collisionShape.RotationDegrees;

			switch (_collisionShape.Shape)
			{
				case BoxShape3D box:
					definition.ShapeType = HitboxShapeType.Box;
					definition.Size = box.Size;
					break;
				case CylinderShape3D cylinder:
					definition.ShapeType = HitboxShapeType.Cylinder;
					definition.Size = new Vector3(cylinder.Radius * 2.0f, cylinder.Height, cylinder.Radius * 2.0f);
					break;
				case SphereShape3D sphere:
					definition.ShapeType = HitboxShapeType.Sphere;
					definition.Size = Vector3.One * sphere.Radius * 2.0f;
					break;
			}
		}

		if (_debugPreview?.Mesh is not null && _debugPreview.MaterialOverride is StandardMaterial3D material)
		{
			definition.DebugColor = material.AlbedoColor;
		}

		definition.ShowDebugPreview = ShowDebugPreview;
		return definition;
	}

	private void ApplyShapeDefinition(ShapeDefinition? definition)
	{
		if (definition == null)
		{
			return;
		}

		if (_collisionShape != null)
		{
			_collisionShape.Position = definition.Offset;
			_collisionShape.RotationDegrees = definition.RotationDegrees;
			_collisionShape.Shape = CreateShape(definition);
		}

		if (_debugPreview != null)
		{
			_debugPreview.Position = definition.Offset;
			_debugPreview.RotationDegrees = definition.RotationDegrees;
			_debugPreview.Mesh = CreateDebugMesh(definition);
			_debugPreview.MaterialOverride = CreateDebugMaterial(definition.DebugColor);
		}
	}

	private void TryApplyHit(Hurtbox hurtbox)
	{
		if (_owner == null || _activeShape == null || _activeContext == null)
		{
			return;
		}

		if (_targetFilter != null && !_targetFilter(hurtbox))
		{
			return;
		}

		int hitCount = _targetHitCounts.GetValueOrDefault(hurtbox, 0);
		int maxHits = _activeShape.MaxHits <= 0 ? int.MaxValue : _activeShape.MaxHits;
		if (hitCount >= maxHits)
		{
			return;
		}

		if (hitCount > 0)
		{
			if (!_activeShape.AllowRepeatHitOnSameTarget)
			{
				return;
			}

			double nextHitTime = _targetNextHitTimes.GetValueOrDefault(hurtbox, 0.0);
			if (_activeElapsedSeconds + 0.0001 < nextHitTime)
			{
				return;
			}
		}

		int nextHitIndex = hitCount + 1;
		_targetHitCounts[hurtbox] = nextHitIndex;
		_targetNextHitTimes[hurtbox] = _activeElapsedSeconds + Mathf.Max(0.01f, _activeShape.RepeatHitInterval);

		if (_combatResolver != null)
		{
			Vector3 hitDirection = -_owner.GlobalTransform.Basis.Z;
			hitDirection.Y = 0.0f;
			if (hitDirection == Vector3.Zero)
			{
				hitDirection = _activeContext.FacingDirection;
			}

			_combatResolver.ResolveHit(new HitResult(
				hurtbox,
				_owner,
				_activeContext.Skill,
				hitDirection.Normalized(),
				nextHitIndex
			));
		}
	}

	private static Shape3D CreateShape(ShapeDefinition definition)
	{
		return definition.ShapeType switch
		{
			HitboxShapeType.Cylinder => new CylinderShape3D
			{
				Radius = Mathf.Max(definition.Size.X, definition.Size.Z) * 0.5f,
				Height = definition.Size.Y
			},
			HitboxShapeType.Sphere => new SphereShape3D
			{
				Radius = Mathf.Max(definition.Size.X, Mathf.Max(definition.Size.Y, definition.Size.Z)) * 0.5f
			},
			_ => new BoxShape3D
			{
				Size = definition.Size
			}
		};
	}

	private static Mesh CreateDebugMesh(ShapeDefinition definition)
	{
		return definition.ShapeType switch
		{
			HitboxShapeType.Cylinder => new CylinderMesh
			{
				TopRadius = Mathf.Max(definition.Size.X, definition.Size.Z) * 0.5f,
				BottomRadius = Mathf.Max(definition.Size.X, definition.Size.Z) * 0.5f,
				Height = definition.Size.Y
			},
			HitboxShapeType.Sphere => new SphereMesh
			{
				Radius = Mathf.Max(definition.Size.X, Mathf.Max(definition.Size.Y, definition.Size.Z)) * 0.5f,
				Height = Mathf.Max(definition.Size.X, Mathf.Max(definition.Size.Y, definition.Size.Z))
			},
			_ => new BoxMesh
			{
				Size = definition.Size
			}
		};
	}

	private static StandardMaterial3D CreateDebugMaterial(Color color)
	{
		return new StandardMaterial3D
		{
			Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
			CullMode = BaseMaterial3D.CullModeEnum.Disabled,
			AlbedoColor = color,
			EmissionEnabled = true,
			Emission = new Color(color.R, color.G, color.B, 1.0f),
			EmissionEnergyMultiplier = 0.6f
		};
	}
}
