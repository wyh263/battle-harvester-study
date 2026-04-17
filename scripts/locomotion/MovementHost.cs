using Godot;

namespace BattleHarvesterStudy;

public partial class MovementHost : Node
{
	private Mover? _mover;
	private ForcedMovementComponent? _forcedMovement;

	public override void _Ready()
	{
		Node? owner = GetOwner();
		if (owner == null)
		{
			return;
		}

		_mover = owner.GetNodeOrNull<Mover>("Components/Mover");
		_forcedMovement = owner.GetNodeOrNull<ForcedMovementComponent>("Components/ForcedMovement");
	}

	public void Move(Vector3 normalDirection, double delta)
	{
		_forcedMovement?.Advance(delta);

		if (_mover == null)
		{
			return;
		}

		if (_forcedMovement?.HasActiveRequest == true)
		{
			_mover.MoveWithTargetSpeed(
				_forcedMovement.CurrentDirection,
				_forcedMovement.CurrentSpeed,
				delta,
				_forcedMovement.CurrentSnapVelocity
			);
			return;
		}

		_mover.Move(normalDirection, delta);
	}
}
