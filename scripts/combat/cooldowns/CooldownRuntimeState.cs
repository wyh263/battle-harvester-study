using Godot;

namespace BattleHarvesterStudy.Combat;

public sealed class CooldownRuntimeState
{
	public CooldownRuntimeState(string id)
	{
		Id = id;
	}

	public string Id { get; }
	public float RemainingSeconds { get; private set; }
	public float TotalDurationSeconds { get; private set; }
	public bool IsReady => RemainingSeconds <= 0.0f;

	public void Start(float duration)
	{
		TotalDurationSeconds = Mathf.Max(0.0f, duration);
		RemainingSeconds = TotalDurationSeconds;
	}

	public void Tick(double delta)
	{
		RemainingSeconds = Mathf.Max(0.0f, RemainingSeconds - (float)delta);
	}

	public void Reduce(float seconds)
	{
		RemainingSeconds = Mathf.Max(0.0f, RemainingSeconds - Mathf.Max(0.0f, seconds));
	}

	public void Refresh()
	{
		RemainingSeconds = 0.0f;
	}
}
