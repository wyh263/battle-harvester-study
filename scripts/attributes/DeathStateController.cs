using Godot;
using Godot.Collections;

namespace BattleHarvesterStudy.Attributes;

public partial class DeathStateController : Node
{
	[Export]
	public string DeathLabelText { get; set; } = "DEAD";

	[Export]
	public Color DeathTint { get; set; } = new(0.35f, 0.35f, 0.35f, 1.0f);

	[Export]
	public Array<NodePath> DisableNodePaths { get; set; } = [];

	private Node3D? _owner;
	private HealthComponent? _health;
	private Targetable? _targetable;
	private Hurtbox? _hurtbox;
	private AttackExecutor? _attackExecutor;
	private ForcedMovementComponent? _forcedMovement;
	private AnimatedSprite3D? _sprite;
	private Label3D? _label;

	public override void _Ready()
	{
		_owner = GetOwner<Node3D>();
		_health = _owner?.GetNodeOrNull<HealthComponent>("Components/Health");
		_targetable = _owner?.GetNodeOrNull<Targetable>("Components/Targetable");
		_hurtbox = _owner?.GetNodeOrNull<Hurtbox>("Hurtbox");
		_attackExecutor = _owner?.GetNodeOrNull<AttackExecutor>("Components/AttackExecutor");
		_forcedMovement = _owner?.GetNodeOrNull<ForcedMovementComponent>("Components/ForcedMovement");
		_sprite = _owner?.GetNodeOrNull<AnimatedSprite3D>("Visuals/AnimatedSprite3D");
		_label = _owner?.GetNodeOrNull<Label3D>("Visuals/DirectionLabel");

		if (_health != null)
		{
			_health.DiedOccurred += OnDied;
			if (_health.IsDead)
			{
				OnDied();
			}
		}
	}

	public override void _ExitTree()
	{
		if (_health != null)
		{
			_health.DiedOccurred -= OnDied;
		}
	}

	private void OnDied()
	{
		_targetable?.SetCanBeTargeted(false);
		if (_hurtbox != null)
		{
			_hurtbox.Monitoring = false;
			_hurtbox.Monitorable = false;
		}

		_attackExecutor?.Cancel();
		_forcedMovement?.Clear();

		if (_owner is IStateActor actor)
		{
			actor.DesiredMoveDirection = Vector3.Zero;
		}

		if (_owner is CharacterBody3D body)
		{
			body.Velocity = Vector3.Zero;
		}

		foreach (NodePath path in DisableNodePaths)
		{
			Node? node = _owner?.GetNodeOrNull(path);
			if (node == null || node == this)
			{
				continue;
			}

			node.ProcessMode = ProcessModeEnum.Disabled;
		}

		if (_sprite != null)
		{
			_sprite.Modulate = DeathTint;
		}

		if (_label != null)
		{
			_label.Text = DeathLabelText;
		}
	}
}
