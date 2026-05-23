using Godot;
using BattleHarvesterStudy.Presentation;

namespace BattleHarvesterStudy.Session;

public partial class ExtractionGate : Node3D
{
	private const string DefaultInteractAction = "interact";

	[Export]
	public NodePath RequesterPath { get; set; } = "../Player";

	[Export]
	public NodePath ControllerPath { get; set; } = "../ExtractionController";

	[Export]
	public NodePath VisualRootPath { get; set; } = "Visuals";

	[Export]
	public NodePath StatusLabelPath { get; set; } = "Visuals/StatusLabel";

	[Export]
	public string InteractAction { get; set; } = DefaultInteractAction;

	[Export(PropertyHint.Range, "0.5,10,0.1")]
	public float RequiredDistance { get; set; } = 2.5f;

	private ExtractionController? _controller;
	private Node3D? _visualRoot;
	private Label3D? _statusLabel;

	public override void _Ready()
	{
		_controller = GetNodeOrNull<ExtractionController>(ControllerPath);
		_visualRoot = GetNodeOrNull<Node3D>(VisualRootPath);
		_statusLabel = GetNodeOrNull<Label3D>(StatusLabelPath);

		if (_controller != null)
		{
			_controller.AvailabilityChanged += OnAvailabilityChanged;
		}

		UiText.LanguageChanged += OnLanguageChanged;
		ApplyAvailability(_controller?.IsAvailable ?? false);
		SetPhysicsProcess(true);
	}

	public override void _ExitTree()
	{
		if (_controller != null)
		{
			_controller.AvailabilityChanged -= OnAvailabilityChanged;
		}

		UiText.LanguageChanged -= OnLanguageChanged;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!Visible || !Input.IsActionJustPressed(InteractAction))
		{
			return;
		}

		Node3D? requester = GetNodeOrNull<Node3D>(RequesterPath);
		if (requester == null || requester.GlobalPosition.DistanceTo(GlobalPosition) > RequiredDistance)
		{
			return;
		}

		RunSession.Instance?.CompleteExtraction(requester);
	}

	private void OnAvailabilityChanged(bool isAvailable)
	{
		ApplyAvailability(isAvailable);
	}

	private void OnLanguageChanged()
	{
		ApplyAvailability(_controller?.IsAvailable ?? false);
	}

	private void ApplyAvailability(bool isAvailable)
	{
		Visible = isAvailable;
		if (_visualRoot != null)
		{
			_visualRoot.Visible = isAvailable;
		}

		if (_statusLabel != null)
		{
			_statusLabel.Text = UiText.CurrentLocale == UiText.DefaultLocale
				? "撤离\n[E]"
				: "Extract\n[E]";
		}
	}
}
