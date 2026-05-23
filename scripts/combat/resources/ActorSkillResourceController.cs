using Godot;

namespace BattleHarvesterStudy.Combat;

public partial class ActorSkillResourceController : Node
{
	[Export]
	public string ResourceLabel { get; set; } = "能量";

	[Export(PropertyHint.Range, "0,999,0.1")]
	public float MaxResource { get; set; } = 100.0f;

	[Export(PropertyHint.Range, "0,999,0.1")]
	public float StartingResource { get; set; } = 100.0f;

	[Export(PropertyHint.Range, "0,999,0.1")]
	public float RegenerationPerSecond { get; set; } = 8.0f;

	public float CurrentResource { get; private set; }

	public override void _Ready()
	{
		CurrentResource = Mathf.Clamp(StartingResource, 0.0f, MaxResource);
		SetPhysicsProcess(true);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (RegenerationPerSecond <= 0.0f || CurrentResource >= MaxResource)
		{
			return;
		}

		CurrentResource = Mathf.Clamp(CurrentResource + (float)delta * RegenerationPerSecond, 0.0f, MaxResource);
	}

	public bool CanSpend(float amount)
	{
		return amount <= 0.0f || CurrentResource + 0.0001f >= amount;
	}

	public bool TrySpend(float amount)
	{
		float clampedAmount = Mathf.Max(0.0f, amount);
		if (!CanSpend(clampedAmount))
		{
			return false;
		}

		CurrentResource = Mathf.Clamp(CurrentResource - clampedAmount, 0.0f, MaxResource);
		return true;
	}

	public void Restore(float amount)
	{
		CurrentResource = Mathf.Clamp(CurrentResource + Mathf.Max(0.0f, amount), 0.0f, MaxResource);
	}

	public void Refill()
	{
		CurrentResource = MaxResource;
	}
}
