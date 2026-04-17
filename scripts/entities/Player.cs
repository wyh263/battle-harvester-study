using Godot;
using System;

namespace BattleHarvesterStudy;

public partial class Player : CharacterBody3D, IHitReactionHost
{
	public const string RunSpeedModifierKey = "state_run";
	private const string BasicSkillPath = "res://resources/combat/skills/basic_attack.tres";
	private const string LaserSkillPath = "res://resources/combat/skills/laser_beam.tres";
	private const string WideBleedSlashSkillPath = "res://resources/combat/skills/wide_bleed_slash.tres";
	private const string SlowOrbSkillPath = "res://resources/combat/skills/slow_orb.tres";

	[Export]
	public SkillDefinition? BasicSkillDefinition { get; set; }

	[Export]
	public SkillDefinition? LaserSkillDefinition { get; set; }

	[Export]
	public SkillDefinition? WideBleedSlashSkillDefinition { get; set; }

	[Export]
	public SkillDefinition? SlowOrbSkillDefinition { get; set; }

	public string FacingLabel { get; set; } = "FRONT";
	public Vector3 DesiredMoveDirection { get; set; } = Vector3.Zero;
	public Vector3 LastMoveWorldDirection { get; private set; } = Vector3.Forward;
	public bool IsInvulnerable { get; private set; }

	private AnimatedSprite3D? _sprite;
	private Label3D? _directionLabel;
	private StatsComponent? _stats;
	private ForcedMovementComponent? _forcedMovement;
	private MovementHost? _movementHost;
	private double _dashCooldownRemaining;
	private double _invulnerableRemaining;
	private SkillDefinition? _queuedSkill;

	public override void _Ready()
	{
		_sprite = GetNodeOrNull<AnimatedSprite3D>("Visuals/AnimatedSprite3D");
		_directionLabel = GetNodeOrNull<Label3D>("Visuals/DirectionLabel");
		_stats = GetNodeOrNull<StatsComponent>("Components/Stats");
		_forcedMovement = GetNodeOrNull<ForcedMovementComponent>("Components/ForcedMovement");
		_movementHost = GetNodeOrNull<MovementHost>("Components/MovementHost");
		EnsurePlaceholderSpriteFrames();
		UpdateDirectionLabel();
		GD.Print("Player Ready");
	}

	public override void _PhysicsProcess(double delta)
	{
		AdvanceTimers(delta);
		_movementHost?.Move(DesiredMoveDirection, delta);
	}

	public bool CanStartDash()
	{
		return _dashCooldownRemaining <= 0.0;
	}

	public bool TryStartDashCooldown()
	{
		if (!CanStartDash())
		{
			return false;
		}

		if (_stats != null)
		{
			_dashCooldownRemaining = _stats.DashCooldown;
		}

		return true;
	}

	public float GetRunMultiplier()
	{
		return _stats?.RunMultiplier ?? 2.0f;
	}

	public float GetDashSpeedMultiplier()
	{
		return _stats?.GetDashSpeedMultiplier() ?? 1.0f;
	}

	public float GetDashSpeed()
	{
		return _stats?.DashSpeed ?? 24.0f;
	}

	public float GetDashDuration()
	{
		return _stats?.DashDuration ?? 0.18f;
	}

	public float GetDashInvulnerableDuration()
	{
		return _stats?.DashInvulnerableDuration ?? 0.10f;
	}

	public SkillDefinition GetBasicSkillDefinition()
	{
		return BasicSkillDefinition ?? LoadRequiredSkill(BasicSkillPath);
	}

	public SkillDefinition GetLaserSkillDefinition()
	{
		return LaserSkillDefinition ?? LoadRequiredSkill(LaserSkillPath);
	}

	public SkillDefinition GetWideBleedSlashSkillDefinition()
	{
		return WideBleedSlashSkillDefinition ?? LoadRequiredSkill(WideBleedSlashSkillPath);
	}

	public SkillDefinition GetSlowOrbSkillDefinition()
	{
		return SlowOrbSkillDefinition ?? LoadRequiredSkill(SlowOrbSkillPath);
	}

	public void QueueSkill(SkillDefinition skill)
	{
		_queuedSkill = skill;
	}

	public SkillDefinition ConsumeQueuedSkillOrDefault()
	{
		SkillDefinition resolved = _queuedSkill ?? GetBasicSkillDefinition();
		_queuedSkill = null;
		return resolved;
	}

	public void SetInvulnerableFor(double duration)
	{
		_invulnerableRemaining = Mathf.Max((float)_invulnerableRemaining, (float)duration);
		IsInvulnerable = _invulnerableRemaining > 0.0;
	}

	public bool TryStartForcedMovement(ForcedMovementRequest request)
	{
		return _forcedMovement?.TryStart(request) ?? false;
	}

	public void ClearForcedMovement()
	{
		_forcedMovement?.Clear();
	}

	public void RememberMoveDirection(Vector3 direction)
	{
		if (direction == Vector3.Zero)
		{
			return;
		}

		Vector3 flattened = direction;
		flattened.Y = 0.0f;
		if (flattened != Vector3.Zero)
		{
			LastMoveWorldDirection = flattened.Normalized();
		}
	}

	public Vector3 GetDashFallbackDirection()
	{
		if (LastMoveWorldDirection != Vector3.Zero)
		{
			return LastMoveWorldDirection;
		}

		Vector3 facing = -GlobalTransform.Basis.Z;
		facing.Y = 0.0f;
		return facing == Vector3.Zero ? Vector3.Forward : facing.Normalized();
	}

	public void SetMoveSpeedModifier(string key, float additive = 0.0f, float multiplier = 1.0f)
	{
		_stats?.SetMoveSpeedModifier(key, additive, multiplier);
	}

	public void RemoveMoveSpeedModifier(string key)
	{
		_stats?.RemoveMoveSpeedModifier(key);
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
	}

	private void UpdateDirectionLabel()
	{
		if (_directionLabel == null)
		{
			return;
		}

		_directionLabel.Text = FacingLabel;
	}

	public string GetHitReactionRestoreLabel()
	{
		return FacingLabel;
	}

	public Color GetHitReactionRestoreColor()
	{
		return Colors.White;
	}

	private void AdvanceTimers(double delta)
	{
		if (_dashCooldownRemaining > 0.0)
		{
			_dashCooldownRemaining = Mathf.Max(0.0f, (float)(_dashCooldownRemaining - delta));
		}

		if (_invulnerableRemaining > 0.0)
		{
			_invulnerableRemaining = Mathf.Max(0.0f, (float)(_invulnerableRemaining - delta));
		}

		IsInvulnerable = _invulnerableRemaining > 0.0;
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
