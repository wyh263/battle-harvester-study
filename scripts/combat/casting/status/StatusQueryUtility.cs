using Godot;

namespace BattleHarvesterStudy.Combat;

public static class StatusQueryUtility
{
	public static bool HasStatus(Node node, string statusId)
	{
		return GetStatusRemaining(node, statusId) > 0.0f;
	}

	public static float GetStatusRemaining(Node node, string statusId)
	{
		return GetStatusRemainingRecursive(node, statusId);
	}

	private static float GetStatusRemainingRecursive(Node node, string statusId)
	{
		float bestRemaining = 0.0f;

		if (node is IStatusQuerySource source && source.HasStatus(statusId))
		{
			bestRemaining = Mathf.Max(bestRemaining, source.GetStatusRemaining(statusId));
		}

		foreach (Node child in node.GetChildren())
		{
			bestRemaining = Mathf.Max(bestRemaining, GetStatusRemainingRecursive(child, statusId));
		}

		return bestRemaining;
	}
}
