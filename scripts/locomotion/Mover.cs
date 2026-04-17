using Godot;

namespace BattleHarvesterStudy;

public partial class Mover : Node
{
	private CharacterBody3D _body = null!;
	private StatsComponent _stats = null!;
	private Vector3 _externalVelocity = Vector3.Zero;

	[Export]
	public float Acceleration { get; set; } = 60.0f;

	[Export]
	public float Friction { get; set; } = 50.0f;

	public override void _Ready()
	{
		_body = GetParent().GetParent<CharacterBody3D>();
		_stats = GetNode<StatsComponent>("../Stats");
	}

	public void Move(Vector3 direction, double delta)
	{
		MoveWithTargetSpeed(direction, _stats.GetCurrentSpeed(), delta);
	}

	public void MoveWithTargetSpeed(Vector3 direction, float targetSpeed, double delta, bool snapHorizontalVelocity = false)
	{
		Vector3 velocity = _body.Velocity;

		if (direction != Vector3.Zero)
		{
			if (snapHorizontalVelocity)
			{
				velocity.X = direction.X * targetSpeed;
				velocity.Z = direction.Z * targetSpeed;
			}
			else
			{
				velocity.X = Mathf.MoveToward(
					velocity.X,
					direction.X * targetSpeed,
					Acceleration * (float)delta
				);

				velocity.Z = Mathf.MoveToward(
					velocity.Z,
					direction.Z * targetSpeed,
					Acceleration * (float)delta
				);
			}
		}
		else
		{
			velocity.X = Mathf.MoveToward(
				velocity.X,
				0.0f,
				Friction * (float)delta
			);

			velocity.Z = Mathf.MoveToward(
				velocity.Z,
				0.0f,
				Friction * (float)delta
			);
		}

		velocity += _externalVelocity;
		_externalVelocity = _externalVelocity.MoveToward(Vector3.Zero, Friction * (float)delta);

		_body.Velocity = velocity;
		_body.MoveAndSlide();
	}

	public void ApplyImpulse(Vector3 force)
	{
		_externalVelocity += force;
	}
}
