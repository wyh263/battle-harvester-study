using Godot;

namespace BattleHarvesterStudy.Targeting;

public partial class Targetable : Node
{
	public const string TargetableGroupName = "targetables";

	[Export]
	public bool CanBeTargeted { get; set; } = true;

	public void SetCanBeTargeted(bool canBeTargeted)
	{
		CanBeTargeted = canBeTargeted;
	}

	public override void _EnterTree()
	{
		AddToGroup(TargetableGroupName);
	}

	public override void _ExitTree()
	{
		RemoveFromGroup(TargetableGroupName);
	}

	public Node3D? GetTargetNode()
	{
		return GetOwner<Node3D>();
	}

	public bool TryGetTargetPosition(out Vector3 position)
	{
		position = Vector3.Zero;
		Node3D? targetNode = GetTargetNode();
		if (targetNode == null || !GodotObject.IsInstanceValid(targetNode))
		{
			return false;
		}

		position = targetNode.GlobalPosition;
		return true;
	}
}
