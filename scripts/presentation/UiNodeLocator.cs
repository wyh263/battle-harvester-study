using Godot;

namespace BattleHarvesterStudy.Presentation;

public static class UiNodeLocator
{
	public static Node3D? ResolveGameplayRoot(Node node)
	{
		for (Node? current = node; current != null; current = current.GetParent())
		{
			if (current is Node3D node3D && node3D.HasNode("Components"))
			{
				return node3D;
			}
		}

		return node.GetOwnerOrNull<Node3D>();
	}
}
