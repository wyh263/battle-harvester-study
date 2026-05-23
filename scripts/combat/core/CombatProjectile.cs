using Godot;

namespace BattleHarvesterStudy.Combat;

public partial class CombatProjectile : Area3D
{
	private readonly CollisionShape3D _collisionShape = new();
	private readonly MeshInstance3D _debugPreview = new();

	private SkillExecutionContext? _context;
	private CombatResolver? _resolver;
	private ShapeDefinition? _shape;
	private ProjectilePresentationDefinition? _presentation;
	private Vector3 _travelDirection = Vector3.Forward;
	private Vector3 _originPosition = Vector3.Zero;
	private float _speed;
	private float _maxDistance;
	private bool _destroyOnFirstHit;
	private bool _hasResolvedHit;
	private bool _hasPublishedDespawn;

	public void Initialize(
		SkillExecutionContext context,
		CombatResolver resolver,
		ShapeDefinition shape,
		float speed,
		float maxDistance,
		bool destroyOnFirstHit,
		ProjectilePresentationDefinition? presentation)
	{
		_context = context;
		_resolver = resolver;
		_shape = shape;
		_presentation = presentation;
		_speed = speed;
		_maxDistance = maxDistance;
		_destroyOnFirstHit = destroyOnFirstHit;
		_travelDirection = context.FacingDirection == Vector3.Zero ? Vector3.Forward : context.FacingDirection.Normalized();
		_originPosition = context.OriginPosition;
		GlobalPosition = context.OriginPosition;
	}

	public override void _Ready()
	{
		AddChild(_collisionShape);
		AddChild(_debugPreview);

		Monitoring = true;
		Monitorable = true;
		SetPhysicsProcess(true);
		AreaEntered += OnAreaEntered;

		ApplyShape();
		PublishProjectilePhase(ProjectilePresentationPhase.Spawned);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_context == null)
		{
			Despawn();
			return;
		}

		GlobalPosition += _travelDirection * _speed * (float)delta;

		if (GlobalPosition.DistanceTo(_originPosition) >= _maxDistance)
		{
			Despawn();
		}
	}

	private void OnAreaEntered(Area3D area)
	{
		if (_hasResolvedHit || area is not Hurtbox hurtbox || _context == null || _resolver == null)
		{
			return;
		}

		if (hurtbox.GetOwner<Node3D>() == _context.Caster)
		{
			return;
		}

		_hasResolvedHit = true;
		PublishProjectilePhase(ProjectilePresentationPhase.Hit, hurtbox);
		_resolver.ResolveHit(new HitResult(
			hurtbox,
			_context.Caster,
			_context.Skill,
			_travelDirection,
			1
		));

		if (_destroyOnFirstHit)
		{
			Despawn();
		}
	}

	private void Despawn()
	{
		PublishProjectilePhase(ProjectilePresentationPhase.Despawned);
		QueueFree();
	}

	private void PublishProjectilePhase(ProjectilePresentationPhase phase, Hurtbox? target = null)
	{
		if (_context == null || _hasPublishedDespawn && phase == ProjectilePresentationPhase.Despawned)
		{
			return;
		}

		if (phase == ProjectilePresentationPhase.Despawned)
		{
			_hasPublishedDespawn = true;
		}

		CombatPresentationEvents.PublishProjectilePhase(
			_context.Caster,
			_context.Skill,
			phase,
			GlobalPosition,
			_travelDirection,
			this,
			target,
			_presentation
		);
	}

	private void ApplyShape()
	{
		ShapeDefinition definition = _shape ?? new ShapeDefinition
		{
			ShapeType = HitboxShapeType.Sphere,
			Size = Vector3.One * 0.8f,
			Offset = Vector3.Zero,
			RotationDegrees = Vector3.Zero,
			DebugColor = new Color(0.3f, 0.9f, 1.0f, 0.35f),
			ShowDebugPreview = true
		};

		_collisionShape.Position = definition.Offset;
		_collisionShape.RotationDegrees = definition.RotationDegrees;
		_collisionShape.Shape = CreateShape(definition);

		_debugPreview.Position = definition.Offset;
		_debugPreview.RotationDegrees = definition.RotationDegrees;
		_debugPreview.Mesh = CreateDebugMesh(definition);
		_debugPreview.MaterialOverride = CreateDebugMaterial(definition.DebugColor);
		_debugPreview.Visible = definition.ShowDebugPreview;
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
			EmissionEnergyMultiplier = 0.8f
		};
	}
}
