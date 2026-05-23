using Godot;

namespace BattleHarvesterStudy.Presentation;

public partial class TargetLockIndicatorPresenter : Node
{
	private const string LockIndicatorNodeName = "LockIndicator";
	private const string LockIndicatorText = "被锁定";

	private CombatAimController? _aimController;

	public override void _Ready()
	{
		Node3D? owner = GetOwner<Node3D>();
		_aimController = owner?.GetNodeOrNull<CombatAimController>("Components/CombatAimController");
		if (_aimController == null)
		{
			return;
		}

		_aimController.LockedTargetStateChanged += OnLockedTargetChanged;
	}

	public override void _ExitTree()
	{
		if (_aimController != null)
		{
			_aimController.LockedTargetStateChanged -= OnLockedTargetChanged;
		}
	}

	private void OnLockedTargetChanged(Node3D? target, bool isLocked)
	{
		if (target == null || !GodotObject.IsInstanceValid(target))
		{
			return;
		}

		Node parent = target.GetNodeOrNull<Node>("Visuals") ?? target;
		Label3D? indicator = parent.GetNodeOrNull<Label3D>(LockIndicatorNodeName);
		if (!isLocked)
		{
			indicator?.QueueFree();
			return;
		}

		if (indicator == null)
		{
			indicator = new Label3D
			{
				Name = LockIndicatorNodeName,
				Text = LockIndicatorText,
				Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
				FontSize = 42,
				OutlineSize = 8,
				Modulate = new Color(1.0f, 0.9f, 0.35f, 1.0f),
				OutlineModulate = new Color(0.08f, 0.08f, 0.08f, 1.0f),
				Position = new Vector3(0.0f, 2.5f, 0.0f)
			};
			parent.AddChild(indicator);
			return;
		}

		indicator.Visible = true;
	}
}
