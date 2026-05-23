using Godot;
using BattleHarvesterStudy.Presentation;

namespace BattleHarvesterStudy.Session;

public partial class RunEntrance : Node3D
{
	private const string DefaultInteractAction = "interact";

	[Export]
	public NodePath RequesterPath { get; set; } = "../Player";

	[Export]
	public NodePath StatusLabelPath { get; set; } = "Visuals/StatusLabel";

	[Export]
	public string InteractAction { get; set; } = DefaultInteractAction;

	[Export]
	public string RunScenePath { get; set; } = "res://scenes/run_map_01.tscn";

	[Export(PropertyHint.Range, "0.5,10,0.1")]
	public float RequiredDistance { get; set; } = 2.5f;

	private Label3D? _statusLabel;

	public override void _Ready()
	{
		_statusLabel = GetNodeOrNull<Label3D>(StatusLabelPath);
		UiText.LanguageChanged += RefreshStatusLabel;
		RefreshStatusLabel();
		SetPhysicsProcess(true);
	}

	public override void _ExitTree()
	{
		UiText.LanguageChanged -= RefreshStatusLabel;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!Input.IsActionJustPressed(InteractAction))
		{
			return;
		}

		Node3D? requester = GetNodeOrNull<Node3D>(RequesterPath);
		if (requester == null || requester.GlobalPosition.DistanceTo(GlobalPosition) > RequiredDistance)
		{
			return;
		}

		RunSession.Instance?.StartRun(requester, RunScenePath);
	}

	private void RefreshStatusLabel()
	{
		if (_statusLabel == null)
		{
			return;
		}

		_statusLabel.Text = UiText.CurrentLocale == UiText.DefaultLocale
			? "出击\n[E]"
			: "Deploy\n[E]";
	}
}
