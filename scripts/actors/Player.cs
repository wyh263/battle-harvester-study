using Godot;
using BattleHarvesterStudy.Presentation;

namespace BattleHarvesterStudy.Actors;

public partial class Player : ActorShell
{
	public const string RunSpeedModifierKey = MovementModifierKeys.Run;

	private FacingLabelController? _facingLabelController;
	private GameplayInputGate? _gameplayInputGate;
	private double _dashCooldownRemaining;
	private double _invulnerableRemaining;

	protected override string InitialFacingLabel => "FRONT";
	public bool IsInvulnerable { get; private set; }
	public override bool IsGameplayInputBlocked => _gameplayInputGate?.BlocksMovementInput ?? false;

	protected override void AfterActorReady()
	{
		_facingLabelController = GetNodeOrNull<FacingLabelController>("Components/FacingLabelController");
		_gameplayInputGate = GetNodeOrNull<GameplayInputGate>("Components/GameplayInputGate");
		GD.Print("Player Ready");
	}

	protected override void BeforeMove(double delta)
	{
		AdvanceTimers(delta);
	}

	public override bool CanStartDash()
	{
		return _dashCooldownRemaining <= 0.0;
	}

	public override bool TryStartDashCooldown()
	{
		if (!CanStartDash())
		{
			return false;
		}

		if (Stats != null)
		{
			_dashCooldownRemaining = Stats.DashCooldown;
		}

		return true;
	}

	public override float GetRunMultiplier()
	{
		return Stats?.RunMultiplier ?? 2.0f;
	}

	public float GetDashSpeedMultiplier()
	{
		return Stats?.GetDashSpeedMultiplier() ?? 1.0f;
	}

	public override float GetDashSpeed()
	{
		return Stats?.DashSpeed ?? 24.0f;
	}

	public override float GetDashDuration()
	{
		return Stats?.DashDuration ?? 0.18f;
	}

	public override float GetDashInvulnerableDuration()
	{
		return Stats?.DashInvulnerableDuration ?? 0.10f;
	}

	public override void SetInvulnerableFor(double duration)
	{
		_invulnerableRemaining = Mathf.Max((float)_invulnerableRemaining, (float)duration);
		IsInvulnerable = _invulnerableRemaining > 0.0;
	}

	protected override void ApplyFacingLabel(string label)
	{
		_facingLabelController?.SetFacingLabel(label);
		base.ApplyFacingLabel(label);
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
}
