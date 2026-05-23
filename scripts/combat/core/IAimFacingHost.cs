using Godot;

namespace BattleHarvesterStudy.Combat;

public interface IAimFacingHost
{
	bool FaceWorldDirection(Vector3 direction);
}
