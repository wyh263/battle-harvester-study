using Godot;
using System;
using System.Threading.Tasks;

namespace BattleHarvesterStudy;

public partial class Dummy : CharacterBody3D, IHitReactionHost
{
	private const string CounterSkillPath = "res://resources/combat/skills/basic_attack.tres";

	private Hurtbox? _hurtbox;
	private AttackExecutor? _attackExecutor;
	private MovementHost? _movementHost;
	private AnimatedSprite3D? _sprite;
	private Label3D? _label;
	private int _retaliationId;
	private Color _displayColor = new(0.70f, 0.88f, 1.0f);
	private string _displayLabel = "DUMMY";

	public override void _Ready()
	{
		_hurtbox = GetNodeOrNull<Hurtbox>("Hurtbox");
		_attackExecutor = GetNodeOrNull<AttackExecutor>("Components/AttackExecutor");
		_movementHost = GetNodeOrNull<MovementHost>("Components/MovementHost");
		_sprite = GetNodeOrNull<AnimatedSprite3D>("Visuals/AnimatedSprite3D");
		_label = GetNodeOrNull<Label3D>("Visuals/DirectionLabel");
		EnsurePlaceholderSpriteFrames();

		if (_hurtbox != null)
		{
			_hurtbox.DamageTaken += OnDamaged;
		}

		ApplyDisplayState("DUMMY", _displayColor);

		GD.Print("Dummy Ready");
	}

	public override void _PhysicsProcess(double delta)
	{
		_movementHost?.Move(Vector3.Zero, delta);
	}

	private void OnDamaged(DamageInfo info)
	{
		_retaliationId++;
		_ = RetaliateAsync(_retaliationId);
	}

	private async Task RetaliateAsync(int retaliationId)
	{
		ApplyDisplayState("ALERT", new Color(1.0f, 0.85f, 0.55f));

		await ToSignal(GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);

		if (!IsInsideTree() || retaliationId != _retaliationId)
		{
			return;
		}

		ApplyDisplayState("COUNTER", new Color(1.0f, 0.55f, 0.55f));
		if (_attackExecutor != null)
		{
			_attackExecutor.Play(LoadRequiredSkill(CounterSkillPath));
			await ToSignal(_attackExecutor, AttackExecutor.SignalName.AttackCompleted);
		}
		else
		{
			await ToSignal(GetTree().CreateTimer(0.12), SceneTreeTimer.SignalName.Timeout);
		}

		if (!IsInsideTree() || retaliationId != _retaliationId)
		{
			return;
		}

		ApplyDisplayState("DUMMY", new Color(0.70f, 0.88f, 1.0f));
	}

	private void EnsurePlaceholderSpriteFrames()
	{
		if (_sprite == null || _sprite.SpriteFrames != null)
		{
			return;
		}

		Texture2D? texture = GD.Load<Texture2D>("res://icon.svg");
		if (texture == null)
		{
			return;
		}

		SpriteFrames frames = new();
		frames.AddAnimation("idle");
		frames.AddFrame("idle", texture);
		frames.SetAnimationLoop("idle", true);
		frames.SetAnimationSpeed("idle", 1.0);

		_sprite.SpriteFrames = frames;
		_sprite.Play("idle");
		_sprite.Modulate = _displayColor;
	}

	private void ApplyDisplayState(string text, Color color)
	{
		_displayLabel = text;
		_displayColor = color;

		if (_label != null)
		{
			_label.Text = _displayLabel;
		}

		if (_sprite != null)
		{
			_sprite.Modulate = _displayColor;
		}
	}

	public string GetHitReactionRestoreLabel()
	{
		return _displayLabel;
	}

	public Color GetHitReactionRestoreColor()
	{
		return _displayColor;
	}

	private static SkillDefinition LoadRequiredSkill(string path)
	{
		Resource? resource = GD.Load<Resource>(path);
		if (resource is SkillDefinition skill)
		{
			return skill;
		}

		throw new InvalidOperationException($"Missing required skill resource: {path}");
	}
}
