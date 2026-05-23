using Godot;
using BattleHarvesterStudy.Session;

namespace BattleHarvesterStudy.Presentation;

public partial class SettlementScreenController : Control
{
	[Export]
	public NodePath TitleLabelPath { get; set; } = new("Margin/Panel/VBox/Title");

	[Export]
	public NodePath LootLabelPath { get; set; } = new("Margin/Panel/VBox/LootValue");

	[Export]
	public NodePath ReturnButtonPath { get; set; } = new("Margin/Panel/VBox/ReturnButton");

	private Label? _titleLabel;
	private Label? _lootLabel;
	private Button? _returnButton;

	public override void _Ready()
	{
		_titleLabel = GetNodeOrNull<Label>(TitleLabelPath);
		_lootLabel = GetNodeOrNull<Label>(LootLabelPath);
		_returnButton = GetNodeOrNull<Button>(ReturnButtonPath);

		if (_returnButton != null)
		{
			_returnButton.Pressed += OnReturnPressed;
		}

		UiText.LanguageChanged += OnLanguageChanged;
		Refresh();
	}

	public override void _ExitTree()
	{
		if (_returnButton != null)
		{
			_returnButton.Pressed -= OnReturnPressed;
		}

		UiText.LanguageChanged -= OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		Refresh();
	}

	private void Refresh()
	{
		bool chinese = UiText.CurrentLocale == UiText.DefaultLocale;
		int lootValue = RunSession.Instance?.LastExtractionLootValue ?? 0;

		if (_titleLabel != null)
		{
			_titleLabel.Text = chinese ? "结算" : "Settlement";
		}

		if (_lootLabel != null)
		{
			_lootLabel.Text = chinese ? $"本局收获  {lootValue}" : $"Run Loot  {lootValue}";
		}

		if (_returnButton != null)
		{
			_returnButton.Text = chinese ? "返回家园" : "Return Home";
		}
	}

	private void OnReturnPressed()
	{
		RunSession? runSession = RunSession.Instance;
		if (runSession == null)
		{
			return;
		}

		runSession.ClearLastExtractionLootValue();
		GetTree().ChangeSceneToFile(runSession.HomeScenePath);
	}
}
