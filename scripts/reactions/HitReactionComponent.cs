using Godot;
using System.Threading.Tasks;

namespace BattleHarvesterStudy.Reactions;

public interface IHitReactionHost
{
	string GetHitReactionRestoreLabel();
	Color GetHitReactionRestoreColor();
}

public partial class HitReactionComponent : Node
{
	[Export]
	public Color HitFlashColor { get; set; } = new(1.0f, 0.45f, 0.45f);

	[Export]
	public string HitLabelText { get; set; } = "HIT";

	[Export]
	public float FeedbackDuration { get; set; } = 0.12f;

	[Export]
	public float KnockbackScale { get; set; } = 1.0f;

	[Export]
	public float KnockbackDuration { get; set; } = 0.10f;

	[Export]
	public int KnockbackPriority { get; set; } = 10;

	private Node3D? _entity;
	private Hurtbox? _hurtbox;
	private Mover? _mover;
	private ForcedMovementComponent? _forcedMovement;
	private AnimatedSprite3D? _sprite;
	private Label3D? _label;
	private int _feedbackId;
	private Color _fallbackSpriteColor = Colors.White;
	private string _fallbackLabelText = "";

	public override void _Ready()
	{
		_entity = GetOwner<Node3D>();
		if (_entity == null)
		{
			return;
		}

		_hurtbox = _entity.GetNodeOrNull<Hurtbox>("Hurtbox");
		_mover = _entity.GetNodeOrNull<Mover>("Components/Mover");
		_forcedMovement = _entity.GetNodeOrNull<ForcedMovementComponent>("Components/ForcedMovement");
		_sprite = _entity.GetNodeOrNull<AnimatedSprite3D>("Visuals/AnimatedSprite3D");
		_label = _entity.GetNodeOrNull<Label3D>("Visuals/DirectionLabel");

		if (_sprite != null)
		{
			_fallbackSpriteColor = _sprite.Modulate;
		}

		if (_label != null)
		{
			_fallbackLabelText = _label.Text;
		}

		if (_hurtbox != null)
		{
			_hurtbox.DamageTaken += OnDamaged;
		}
	}

	public override void _ExitTree()
	{
		if (_hurtbox != null)
		{
			_hurtbox.DamageTaken -= OnDamaged;
		}
	}

	private void OnDamaged(DamageInfo info)
	{
		Vector3 knockback = info.Knockback;

		if (knockback != Vector3.Zero)
		{
			Vector3 scaledKnockback = knockback * KnockbackScale;
			float forcedMovementDuration = info.ForcedMovementDuration > 0.0f ? info.ForcedMovementDuration : KnockbackDuration;
			int forcedMovementPriority = info.ForcedMovementPriority > 0 ? info.ForcedMovementPriority : KnockbackPriority;
			bool shouldUseForcedMovement = info.CausesForcedMovement;
			bool handledByForcedMovement = _forcedMovement?.TryStart(new ForcedMovementRequest
			{
				SourceKey = string.IsNullOrEmpty(info.AttackId) ? "hit_knockback" : $"hit_{info.AttackId}",
				Direction = scaledKnockback,
				Speed = scaledKnockback.Length(),
				Duration = forcedMovementDuration,
				SnapVelocity = true,
				LockInput = true,
				Priority = forcedMovementPriority
			}) ?? false;

			if (shouldUseForcedMovement && !handledByForcedMovement && _mover != null)
			{
				_mover.ApplyImpulse(scaledKnockback);
			}
		}

		_feedbackId++;
		_ = PlayFeedbackAsync(_feedbackId);
	}

	private async Task PlayFeedbackAsync(int feedbackId)
	{
		if (_label != null)
		{
			_label.Text = HitLabelText;
		}

		if (_sprite != null)
		{
			_sprite.Modulate = HitFlashColor;
		}

		await ToSignal(GetTree().CreateTimer(FeedbackDuration), SceneTreeTimer.SignalName.Timeout);

		if (!IsInsideTree() || feedbackId != _feedbackId)
		{
			return;
		}

		if (_sprite != null)
		{
			_sprite.Modulate = ResolveRestoreColor();
		}

		if (_label != null)
		{
			_label.Text = ResolveRestoreLabel();
		}
	}

	private string ResolveRestoreLabel()
	{
		if (_entity is IHitReactionHost host)
		{
			return host.GetHitReactionRestoreLabel();
		}

		return _fallbackLabelText;
	}

	private Color ResolveRestoreColor()
	{
		if (_entity is IHitReactionHost host)
		{
			return host.GetHitReactionRestoreColor();
		}

		return _fallbackSpriteColor;
	}
}
