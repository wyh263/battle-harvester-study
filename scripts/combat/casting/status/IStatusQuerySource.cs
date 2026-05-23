namespace BattleHarvesterStudy.Combat;

public interface IStatusQuerySource
{
	bool HasStatus(string statusId);
	float GetStatusRemaining(string statusId);
}
