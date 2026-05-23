using Godot;
using System;
using BattleHarvesterStudy.Reactions;

namespace BattleHarvesterStudy.Actors;

public abstract partial class ActorShell : CharacterBody3D, IHitReactionHost, IAimFacingHost, IStateActor
{
	private string _facingLabel = "";

	public string FacingLabel
	{
		get => _facingLabel;
		set
		{
			_facingLabel = value;
			ApplyFacingLabel(value);
		}
	}

	public Vector3 DesiredMoveDirection { get; set; } = Vector3.Zero;
	public Vector3 LastMoveWorldDirection { get; private set; } = Vector3.Forward;
	public ActorSkillLoadout SkillLoadout { get; protected set; } = null!;
	public ActorSkillCooldownController SkillCooldowns { get; protected set; } = null!;
	public SkillChainTracker SkillChainTracker { get; protected set; } = null!;

	protected AnimatedSprite3D? Sprite { get; private set; }
	protected Label3D? DirectionLabel { get; private set; }
	protected StatsComponent? Stats { get; private set; }
	protected ForcedMovementComponent? ForcedMovement { get; private set; }
	protected MovementHost? MovementHost { get; private set; }

	protected abstract string InitialFacingLabel { get; }
	protected virtual Color DefaultSpriteColor => Colors.White;
	public abstract bool IsGameplayInputBlocked { get; }

	public override void _Ready()
	{
		Sprite = GetNodeOrNull<AnimatedSprite3D>("Visuals/AnimatedSprite3D");
		DirectionLabel = GetNodeOrNull<Label3D>("Visuals/DirectionLabel");
		Stats = GetNodeOrNull<StatsComponent>("Components/Stats");
		ForcedMovement = GetNodeOrNull<ForcedMovementComponent>("Components/ForcedMovement");
		MovementHost = GetNodeOrNull<MovementHost>("Components/MovementHost");
		SkillLoadout = GetNodeOrNull<ActorSkillLoadout>("Components/SkillLoadout")
			?? throw new InvalidOperationException($"{GetType().Name} requires Components/SkillLoadout");
		SkillCooldowns = GetNodeOrNull<ActorSkillCooldownController>("Components/SkillCooldowns")
			?? throw new InvalidOperationException($"{GetType().Name} requires Components/SkillCooldowns");
		SkillChainTracker = GetNodeOrNull<SkillChainTracker>("Components/SkillChainTracker")
			?? throw new InvalidOperationException($"{GetType().Name} requires Components/SkillChainTracker");

		EnsurePlaceholderSpriteFrames();
		if (Sprite != null)
		{
			Sprite.Modulate = DefaultSpriteColor;
		}

		FacingLabel = InitialFacingLabel;
		AfterActorReady();
	}

	public override void _PhysicsProcess(double delta)
	{
		BeforeMove(delta);
		MovementHost?.Move(DesiredMoveDirection, delta);
		AfterMove(delta);
	}

	public abstract bool CanStartDash();
	public abstract bool TryStartDashCooldown();
	public abstract float GetRunMultiplier();
	public abstract float GetDashSpeed();
	public abstract float GetDashDuration();
	public abstract float GetDashInvulnerableDuration();
	public abstract void SetInvulnerableFor(double duration);

	public bool TryStartForcedMovement(ForcedMovementRequest request)
	{
		return ForcedMovement?.TryStart(request) ?? false;
	}

	public void ClearForcedMovement()
	{
		ForcedMovement?.Clear();
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
		Stats?.SetMoveSpeedModifier(key, additive, multiplier);
	}

	public void RemoveMoveSpeedModifier(string key)
	{
		Stats?.RemoveMoveSpeedModifier(key);
	}

	public bool FaceWorldDirection(Vector3 direction)
	{
		if (direction == Vector3.Zero)
		{
			return false;
		}

		Vector3 flattened = direction;
		flattened.Y = 0.0f;
		if (flattened == Vector3.Zero)
		{
			return false;
		}

		float targetAngle = Mathf.Atan2(-flattened.X, -flattened.Z);
		Rotation = new Vector3(Rotation.X, targetAngle, Rotation.Z);
		RememberMoveDirection(flattened);
		return true;
	}

	public string GetHitReactionRestoreLabel()
	{
		return FacingLabel;
	}

	public Color GetHitReactionRestoreColor()
	{
		return DefaultSpriteColor;
	}

	protected virtual void AfterActorReady()
	{
	}

	protected virtual void BeforeMove(double delta)
	{
	}

	protected virtual void AfterMove(double delta)
	{
	}

	protected virtual void ApplyFacingLabel(string label)
	{
		if (DirectionLabel != null)
		{
			DirectionLabel.Text = label;
		}
	}

	private void EnsurePlaceholderSpriteFrames()
	{
		if (Sprite == null || Sprite.SpriteFrames != null)
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

		Sprite.SpriteFrames = frames;
		Sprite.Play("idle");
	}
}
