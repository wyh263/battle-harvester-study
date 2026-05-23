using Godot;
using BattleHarvesterStudy.Session;

namespace BattleHarvesterStudy.Presentation;

public partial class EconomyPanelPresenter : Node
{
	[Export]
	public NodePath CreditsLabelPath { get; set; } = new("../../TopRight/CreditsPanel/Margin/CreditsLabel");

	private Label? _creditsLabel;

	public override void _Ready()
	{
		_creditsLabel = GetNodeOrNull<Label>(CreditsLabelPath);
		SetProcess(true);
	}

	public override void _Process(double delta)
	{
		if (_creditsLabel == null)
		{
			return;
		}

		int credits = RunSession.Instance?.PlayerCredits ?? 0;
		bool chinese = UiText.CurrentLocale == UiText.DefaultLocale;
		_creditsLabel.Text = chinese ? $"资金  {credits}" : $"Credits  {credits}";
	}
}
