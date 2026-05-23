using Godot;

namespace BattleHarvesterStudy.Actors;

public partial class Dummy : ActorShell
{
	private readonly Color _displayColor = new(0.70f, 0.88f, 1.0f);

	protected override string InitialFacingLabel => "DUMMY";
	protected override Color DefaultSpriteColor => _displayColor;
	public override bool IsGameplayInputBlocked => false;

	protected override void AfterActorReady()
	{
		GD.Print("Dummy Ready");
	}

	public override bool CanStartDash()
	{
		return false;
	}

	public override bool TryStartDashCooldown()
	{
		return false;
	}

	public override float GetRunMultiplier()
	{
		return 1.0f;
	}

	public override float GetDashSpeed()
	{
		return 0.0f;
	}

	public override float GetDashDuration()
	{
		return 0.0f;
	}

	public override float GetDashInvulnerableDuration()
	{
		return 0.0f;
	}

	public override void SetInvulnerableFor(double duration)
	{
	}
}
