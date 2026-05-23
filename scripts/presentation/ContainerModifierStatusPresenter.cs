using Godot;
using System.Collections.Generic;
using BattleHarvesterStudy.Inventory;
using BattleHarvesterStudy.Inventory.Search;

namespace BattleHarvesterStudy.Presentation;

public partial class ContainerModifierStatusPresenter : Node
{
	[Export]
	public NodePath UiContextPath { get; set; } = new("../PlayerUiContext");

	[Export]
	public NodePath StatusRowPath { get; set; } = new("../../TopLeft/TopPanel/Margin/VBox/SearchStatusRow");

	[Export]
	public NodePath TooltipPanelPath { get; set; } = new("../../TopLeft/SearchStatusTooltip");

	[Export]
	public NodePath TooltipLabelPath { get; set; } = new("../../TopLeft/SearchStatusTooltip/Margin/Text");

	private PlayerUiContext? _uiContext;
	private HBoxContainer? _statusRow;
	private PanelContainer? _tooltipPanel;
	private Label? _tooltipLabel;
	private readonly List<Button> _buttons = [];
	private int _lastCount = -1;
	private string _lastLocale = string.Empty;

	public override void _Ready()
	{
		_uiContext = GetNodeOrNull<PlayerUiContext>(UiContextPath);
		_statusRow = GetNodeOrNull<HBoxContainer>(StatusRowPath);
		_tooltipPanel = GetNodeOrNull<PanelContainer>(TooltipPanelPath);
		_tooltipLabel = GetNodeOrNull<Label>(TooltipLabelPath);
		HideTooltip();
	}

	public override void _Process(double delta)
	{
		RefreshIfNeeded();
	}

	private void RefreshIfNeeded()
	{
		if (_statusRow == null)
		{
			return;
		}

		IReadOnlyList<ContainerModifierProfileDefinition>? profiles = _uiContext?.ActiveContainerModifiers?.GetActiveProfiles();
		int count = profiles?.Count ?? 0;
		if (count == _lastCount && _lastLocale == UiText.CurrentLocale)
		{
			return;
		}

		_lastCount = count;
		_lastLocale = UiText.CurrentLocale;
		Rebuild(profiles);
	}

	private void Rebuild(IReadOnlyList<ContainerModifierProfileDefinition>? profiles)
	{
		if (_statusRow == null)
		{
			return;
		}

		foreach (Button button in _buttons)
		{
			button.QueueFree();
		}

		_buttons.Clear();
		HideTooltip();
		int count = profiles?.Count ?? 0;
		_statusRow.Visible = count > 0;
		if (count <= 0 || profiles == null)
		{
			return;
		}

		foreach (ContainerModifierProfileDefinition profile in profiles)
		{
			Button button = new()
			{
				CustomMinimumSize = new Vector2(28, 28),
				Text = GetIconText(profile),
				FocusMode = Control.FocusModeEnum.None
			};
			string tooltip = BuildTooltip(profile);
			button.MouseEntered += () => ShowTooltip(tooltip);
			button.MouseExited += HideTooltip;
			_statusRow.AddChild(button);
			_buttons.Add(button);
		}
	}

	private static string GetIconText(ContainerModifierProfileDefinition profile)
	{
		bool hasSearch = !Mathf.IsEqualApprox(profile.GlobalSearchSpeedMultiplier, 1.0f);
		bool hasLoot = HasLootBias(profile);
		if (hasSearch && hasLoot)
		{
			return UiText.CurrentLocale == UiText.DefaultLocale ? "综" : "A";
		}

		if (hasSearch)
		{
			return UiText.CurrentLocale == UiText.DefaultLocale ? "搜" : "S";
		}

		if (hasLoot)
		{
			return UiText.CurrentLocale == UiText.DefaultLocale ? "爆" : "L";
		}

		return UiText.CurrentLocale == UiText.DefaultLocale ? "效" : "E";
	}

	private static string BuildTooltip(ContainerModifierProfileDefinition profile)
	{
		bool chinese = UiText.CurrentLocale == UiText.DefaultLocale;
		List<string> lines = [profile.DisplayName];

		if (!Mathf.IsEqualApprox(profile.GlobalSearchSpeedMultiplier, 1.0f))
		{
			lines.Add(chinese
				? $"搜索速度 x{profile.GlobalSearchSpeedMultiplier:0.##}"
				: $"Search speed x{profile.GlobalSearchSpeedMultiplier:0.##}");
		}

		if (HasLootBias(profile))
		{
			lines.Add(chinese ? "高阶颜色权重" : "High rarity weights");
			AppendRarityLine(lines, chinese, "蓝", "Blue", profile.BlueLootWeightMultiplier);
			AppendRarityLine(lines, chinese, "紫", "Purple", profile.PurpleLootWeightMultiplier);
			AppendRarityLine(lines, chinese, "金", "Gold", profile.GoldLootWeightMultiplier);
			AppendRarityLine(lines, chinese, "红", "Red", profile.RedLootWeightMultiplier);
		}

		return string.Join("\n", lines);
	}

	private static bool HasLootBias(ContainerModifierProfileDefinition profile)
	{
		return !Mathf.IsEqualApprox(profile.GlobalLootWeightMultiplier, 1.0f)
			|| !Mathf.IsEqualApprox(profile.WhiteLootWeightMultiplier, 1.0f)
			|| !Mathf.IsEqualApprox(profile.GreenLootWeightMultiplier, 1.0f)
			|| !Mathf.IsEqualApprox(profile.BlueLootWeightMultiplier, 1.0f)
			|| !Mathf.IsEqualApprox(profile.PurpleLootWeightMultiplier, 1.0f)
			|| !Mathf.IsEqualApprox(profile.GoldLootWeightMultiplier, 1.0f)
			|| !Mathf.IsEqualApprox(profile.RedLootWeightMultiplier, 1.0f);
	}

	private static void AppendRarityLine(List<string> lines, bool chinese, string chineseLabel, string englishLabel, float value)
	{
		if (Mathf.IsEqualApprox(value, 1.0f))
		{
			return;
		}

		lines.Add(chinese ? $"{chineseLabel} x{value:0.##}" : $"{englishLabel} x{value:0.##}");
	}

	private void ShowTooltip(string text)
	{
		if (_tooltipPanel == null || _tooltipLabel == null)
		{
			return;
		}

		_tooltipLabel.Text = text;
		_tooltipPanel.Visible = true;
	}

	private void HideTooltip()
	{
		if (_tooltipPanel == null)
		{
			return;
		}

		_tooltipPanel.Visible = false;
	}
}
