using Godot;
using System.Collections.Generic;

namespace BattleHarvesterStudy.Presentation;

public partial class SettingsMenuController : Node
{
	private const string SettingsConfigPath = "user://settings.cfg";
	private const string SettingsSection = "settings";
	private const string BindingsSection = "bindings";
	private const string ToggleAction = "ui_cancel";

	private sealed record RebindRow(string ActionName, string ZhLabel, string EnLabel, Label NameLabel, Label BindingLabel, Button RebindButton);

	private static readonly (string ActionName, string ZhLabel, string EnLabel)[] RebindActions =
	[
		("move_up", "上移", "Move Up"),
		("move_down", "下移", "Move Down"),
		("move_left", "左移", "Move Left"),
		("move_right", "右移", "Move Right"),
		("attack", "普通攻击", "Attack"),
		("dash", "闪避", "Dash"),
		("interact", "交互", "Interact"),
		("toggle_inventory", "背包", "Inventory"),
		("skill_u", "技能槽 1", "Skill Slot 1"),
		("skill_i", "技能槽 2", "Skill Slot 2"),
		("skill_o", "技能槽 3", "Skill Slot 3"),
		("skill_p", "技能槽 4", "Skill Slot 4"),
		("item_slot_1", "道具槽 1", "Item Slot 1"),
		("item_slot_2", "道具槽 2", "Item Slot 2"),
		("item_slot_3", "道具槽 3", "Item Slot 3"),
		("weapon_slot_1", "武器槽 1", "Weapon Slot 1"),
		("weapon_slot_2", "武器槽 2", "Weapon Slot 2"),
		("weapon_swap", "切换武器", "Swap Weapon"),
		("lock_enter", "锁定目标", "Lock Target"),
		("lock_exit", "取消锁定", "Unlock Target"),
		("toggle_firearm_debug", "枪械调试面板", "Firearm Debug Panel"),
	];

	private static readonly (Vector2I Size, string ZhLabel, string EnLabel)[] ResolutionOptions =
	[
		(new Vector2I(1280, 720), "1280 x 720", "1280 x 720"),
		(new Vector2I(1600, 900), "1600 x 900", "1600 x 900"),
		(new Vector2I(1920, 1080), "1920 x 1080", "1920 x 1080"),
		(new Vector2I(2560, 1440), "2560 x 1440", "2560 x 1440"),
	];

	[Export] public NodePath OverlayPath { get; set; } = new("../../SettingsOverlay");
	[Export] public NodePath InventoryUiControllerPath { get; set; } = new("../InventoryUiController");
	[Export] public NodePath TitleLabelPath { get; set; } = new("../../SettingsOverlay/Window/Margin/VBox/HeaderBar/Title");
	[Export] public NodePath CloseButtonPath { get; set; } = new("../../SettingsOverlay/Window/Margin/VBox/HeaderBar/CloseButton");
	[Export] public NodePath TabsPath { get; set; } = new("../../SettingsOverlay/Window/Margin/VBox/Tabs");
	[Export] public NodePath ControlsTabPath { get; set; } = new("../../SettingsOverlay/Window/Margin/VBox/Tabs/ControlsTab");
	[Export] public NodePath ControlsListPath { get; set; } = new("../../SettingsOverlay/Window/Margin/VBox/Tabs/ControlsTab/Margin/Scroll/ActionList");
	[Export] public NodePath ControlsHintPath { get; set; } = new("../../SettingsOverlay/Window/Margin/VBox/Tabs/ControlsTab/Margin/ControlsHint");
	[Export] public NodePath LanguageTabPath { get; set; } = new("../../SettingsOverlay/Window/Margin/VBox/Tabs/LanguageTab");
	[Export] public NodePath LanguageLabelPath { get; set; } = new("../../SettingsOverlay/Window/Margin/VBox/Tabs/LanguageTab/Margin/VBox/LanguageLabel");
	[Export] public NodePath LanguageOptionPath { get; set; } = new("../../SettingsOverlay/Window/Margin/VBox/Tabs/LanguageTab/Margin/VBox/LanguageOption");
	[Export] public NodePath GraphicsTabPath { get; set; } = new("../../SettingsOverlay/Window/Margin/VBox/Tabs/GraphicsTab");
	[Export] public NodePath GraphicsLabelPath { get; set; } = new("../../SettingsOverlay/Window/Margin/VBox/Tabs/GraphicsTab/Margin/VBox/GraphicsLabel");
	[Export] public NodePath GraphicsModeOptionPath { get; set; } = new("../../SettingsOverlay/Window/Margin/VBox/Tabs/GraphicsTab/Margin/VBox/GraphicsModeOption");
	[Export] public NodePath ResolutionLabelPath { get; set; } = new("../../SettingsOverlay/Window/Margin/VBox/Tabs/GraphicsTab/Margin/VBox/ResolutionLabel");
	[Export] public NodePath ResolutionOptionPath { get; set; } = new("../../SettingsOverlay/Window/Margin/VBox/Tabs/GraphicsTab/Margin/VBox/ResolutionOption");
	[Export] public NodePath VsyncLabelPath { get; set; } = new("../../SettingsOverlay/Window/Margin/VBox/Tabs/GraphicsTab/Margin/VBox/VsyncLabel");
	[Export] public NodePath VsyncTogglePath { get; set; } = new("../../SettingsOverlay/Window/Margin/VBox/Tabs/GraphicsTab/Margin/VBox/VsyncToggle");
	[Export] public NodePath FpsLabelPath { get; set; } = new("../../SettingsOverlay/Window/Margin/VBox/Tabs/GraphicsTab/Margin/VBox/FpsLabel");
	[Export] public NodePath FpsSpinPath { get; set; } = new("../../SettingsOverlay/Window/Margin/VBox/Tabs/GraphicsTab/Margin/VBox/FpsSpin");

	private Control? _overlay;
	private InventoryUiController? _inventoryUiController;
	private Label? _titleLabel;
	private Button? _closeButton;
	private TabContainer? _tabs;
	private Control? _controlsTab;
	private VBoxContainer? _controlsList;
	private Label? _controlsHint;
	private Control? _languageTab;
	private Label? _languageLabel;
	private OptionButton? _languageOption;
	private Control? _graphicsTab;
	private Label? _graphicsLabel;
	private OptionButton? _graphicsModeOption;
	private Label? _resolutionLabel;
	private OptionButton? _resolutionOption;
	private Label? _vsyncLabel;
	private CheckButton? _vsyncToggle;
	private Label? _fpsLabel;
	private SpinBox? _fpsSpin;
	private readonly List<RebindRow> _rebindRows = [];
	private string? _awaitingRebindAction;
	private bool _suppressOptionCallbacks;

	public bool IsOpen => _overlay?.Visible ?? false;

	public GameplayInputBlockState GetGameplayInputBlockState()
	{
		bool blocked = IsOpen;
		return new GameplayInputBlockState(blocked, blocked, blocked, blocked, blocked);
	}

	public override void _Ready()
	{
		_inventoryUiController = GetNodeOrNull<InventoryUiController>(InventoryUiControllerPath);
		_overlay = GetNodeOrNull<Control>(OverlayPath);
		_titleLabel = GetNodeOrNull<Label>(TitleLabelPath);
		_closeButton = GetNodeOrNull<Button>(CloseButtonPath);
		_tabs = GetNodeOrNull<TabContainer>(TabsPath);
		_controlsTab = GetNodeOrNull<Control>(ControlsTabPath);
		_controlsList = GetNodeOrNull<VBoxContainer>(ControlsListPath);
		_controlsHint = GetNodeOrNull<Label>(ControlsHintPath);
		_languageTab = GetNodeOrNull<Control>(LanguageTabPath);
		_languageLabel = GetNodeOrNull<Label>(LanguageLabelPath);
		_languageOption = GetNodeOrNull<OptionButton>(LanguageOptionPath);
		_graphicsTab = GetNodeOrNull<Control>(GraphicsTabPath);
		_graphicsLabel = GetNodeOrNull<Label>(GraphicsLabelPath);
		_graphicsModeOption = GetNodeOrNull<OptionButton>(GraphicsModeOptionPath);
		_resolutionLabel = GetNodeOrNull<Label>(ResolutionLabelPath);
		_resolutionOption = GetNodeOrNull<OptionButton>(ResolutionOptionPath);
		_vsyncLabel = GetNodeOrNull<Label>(VsyncLabelPath);
		_vsyncToggle = GetNodeOrNull<CheckButton>(VsyncTogglePath);
		_fpsLabel = GetNodeOrNull<Label>(FpsLabelPath);
		_fpsSpin = GetNodeOrNull<SpinBox>(FpsSpinPath);

		if (_overlay != null)
		{
			_overlay.Visible = false;
		}

		if (_closeButton != null)
		{
			_closeButton.Pressed += CloseMenu;
		}

		UiText.LanguageChanged += OnLanguageChanged;

		BuildControlsList();
		InitializeLanguageOptions();
		InitializeGraphicsOptions();
		LoadSettings();
		RefreshUiText();

		SetProcess(true);
		SetProcessUnhandledInput(true);
	}

	public override void _ExitTree()
	{
		if (_closeButton != null)
		{
			_closeButton.Pressed -= CloseMenu;
		}

		if (_languageOption != null)
		{
			_languageOption.ItemSelected -= OnLanguageSelected;
		}

		if (_graphicsModeOption != null)
		{
			_graphicsModeOption.ItemSelected -= OnGraphicsModeSelected;
		}

		if (_resolutionOption != null)
		{
			_resolutionOption.ItemSelected -= OnResolutionSelected;
		}

		if (_vsyncToggle != null)
		{
			_vsyncToggle.Toggled -= OnVsyncToggled;
		}

		if (_fpsSpin != null)
		{
			_fpsSpin.ValueChanged -= OnFpsValueChanged;
		}

		UiText.LanguageChanged -= OnLanguageChanged;
	}

	public override void _Process(double delta)
	{
		if (!Input.IsActionJustPressed(ToggleAction))
		{
			return;
		}

		if (_awaitingRebindAction != null)
		{
			CancelRebind();
			return;
		}

		if (IsOpen)
		{
			CloseMenu();
		}
		else if (_inventoryUiController?.TryHandleCancelAction() == true)
		{
			return;
		}
		else
		{
			OpenMenu();
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (_awaitingRebindAction == null || @event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
		{
			return;
		}

		GetViewport().SetInputAsHandled();
		if (keyEvent.Keycode == Key.Escape)
		{
			CancelRebind();
			return;
		}

		RebindAction(_awaitingRebindAction, keyEvent);
	}

	private void OpenMenu()
	{
		if (_overlay == null)
		{
			return;
		}

		_overlay.Visible = true;
		RefreshUiText();
	}

	private void CloseMenu()
	{
		if (_overlay == null)
		{
			return;
		}

		_overlay.Visible = false;
		CancelRebind();
	}

	private void BuildControlsList()
	{
		if (_controlsList == null)
		{
			return;
		}

		foreach (Node child in _controlsList.GetChildren())
		{
			child.QueueFree();
		}

		_rebindRows.Clear();
		foreach ((string actionName, string zhLabel, string enLabel) in RebindActions)
		{
			HBoxContainer row = new();
			row.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

			Label nameLabel = new();
			nameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

			Label bindingLabel = new();
			bindingLabel.CustomMinimumSize = new Vector2(140.0f, 0.0f);
			bindingLabel.HorizontalAlignment = HorizontalAlignment.Right;

			Button rebindButton = new();
			rebindButton.CustomMinimumSize = new Vector2(96.0f, 30.0f);
			string capturedActionName = actionName;
			rebindButton.Pressed += () => BeginRebind(capturedActionName);

			row.AddChild(nameLabel);
			row.AddChild(bindingLabel);
			row.AddChild(rebindButton);
			_controlsList.AddChild(row);

			_rebindRows.Add(new RebindRow(actionName, zhLabel, enLabel, nameLabel, bindingLabel, rebindButton));
		}

		RefreshBindingLabels();
	}

	private void InitializeLanguageOptions()
	{
		if (_languageOption == null)
		{
			return;
		}

		_languageOption.Clear();
		_languageOption.AddItem("简体中文");
		_languageOption.SetItemMetadata(0, UiText.DefaultLocale);
		_languageOption.AddItem("English");
		_languageOption.SetItemMetadata(1, "en");
		_languageOption.ItemSelected += OnLanguageSelected;
	}

	private void InitializeGraphicsOptions()
	{
		if (_graphicsModeOption != null)
		{
			_graphicsModeOption.Clear();
			_graphicsModeOption.AddItem("窗口");
			_graphicsModeOption.SetItemMetadata(0, (int)DisplayServer.WindowMode.Windowed);
			_graphicsModeOption.AddItem("全屏");
			_graphicsModeOption.SetItemMetadata(1, (int)DisplayServer.WindowMode.Fullscreen);
			_graphicsModeOption.AddItem("独占全屏");
			_graphicsModeOption.SetItemMetadata(2, (int)DisplayServer.WindowMode.ExclusiveFullscreen);
			_graphicsModeOption.ItemSelected += OnGraphicsModeSelected;
		}

		if (_resolutionOption != null)
		{
			_resolutionOption.Clear();
			for (int i = 0; i < ResolutionOptions.Length; i++)
			{
				_resolutionOption.AddItem(ResolutionOptions[i].ZhLabel);
				_resolutionOption.SetItemMetadata(i, $"{ResolutionOptions[i].Size.X}x{ResolutionOptions[i].Size.Y}");
			}

			_resolutionOption.ItemSelected += OnResolutionSelected;
		}

		if (_vsyncToggle != null)
		{
			_vsyncToggle.Toggled += OnVsyncToggled;
		}

		if (_fpsSpin != null)
		{
			_fpsSpin.MinValue = 0;
			_fpsSpin.MaxValue = 240;
			_fpsSpin.Step = 30;
			_fpsSpin.ValueChanged += OnFpsValueChanged;
		}
	}

	private void LoadSettings()
	{
		ConfigFile config = new();
		if (config.Load(SettingsConfigPath) == Error.Ok)
		{
			UiText.SetLocale(config.GetValue(SettingsSection, "language", UiText.DefaultLocale).AsString());
			ApplyWindowMode((int)config.GetValue(SettingsSection, "window_mode", (int)DisplayServer.WindowGetMode()));
			ApplyResolution(new Vector2I(
				(int)config.GetValue(SettingsSection, "resolution_width", DisplayServer.WindowGetSize().X),
				(int)config.GetValue(SettingsSection, "resolution_height", DisplayServer.WindowGetSize().Y)));
			ApplyVsync((bool)config.GetValue(SettingsSection, "vsync", true));
			ApplyMaxFps((int)config.GetValue(SettingsSection, "max_fps", 0));

			foreach ((string actionName, _, _) in RebindActions)
			{
				string keycodeKey = $"{actionName}_keycode";
				if (!config.HasSectionKey(BindingsSection, keycodeKey))
				{
					continue;
				}

				InputEventKey keyEvent = CreateKeyEventFromConfig(config, actionName);
				InputMap.ActionEraseEvents(actionName);
				InputMap.ActionAddEvent(actionName, keyEvent);
			}
		}

		ApplyRuntimeSelections();
		RefreshBindingLabels();
	}

	private void SaveSettings()
	{
		ConfigFile config = new();
		config.SetValue(SettingsSection, "language", UiText.CurrentLocale);
		config.SetValue(SettingsSection, "window_mode", (int)DisplayServer.WindowGetMode());
		config.SetValue(SettingsSection, "resolution_width", DisplayServer.WindowGetSize().X);
		config.SetValue(SettingsSection, "resolution_height", DisplayServer.WindowGetSize().Y);
		config.SetValue(SettingsSection, "vsync", DisplayServer.WindowGetVsyncMode() != DisplayServer.VSyncMode.Disabled);
		config.SetValue(SettingsSection, "max_fps", Engine.MaxFps);

		foreach ((string actionName, _, _) in RebindActions)
		{
			InputEventKey? keyEvent = GetFirstKeyEvent(actionName);
			if (keyEvent == null)
			{
				continue;
			}

			config.SetValue(BindingsSection, $"{actionName}_keycode", (int)keyEvent.Keycode);
			config.SetValue(BindingsSection, $"{actionName}_physical_keycode", (int)keyEvent.PhysicalKeycode);
			config.SetValue(BindingsSection, $"{actionName}_shift", keyEvent.ShiftPressed);
			config.SetValue(BindingsSection, $"{actionName}_ctrl", keyEvent.CtrlPressed);
			config.SetValue(BindingsSection, $"{actionName}_alt", keyEvent.AltPressed);
			config.SetValue(BindingsSection, $"{actionName}_meta", keyEvent.MetaPressed);
		}

		config.Save(SettingsConfigPath);
	}

	private static InputEventKey CreateKeyEventFromConfig(ConfigFile config, string actionName)
	{
		return new InputEventKey
		{
			Keycode = (Key)(int)config.GetValue(BindingsSection, $"{actionName}_keycode", (int)Key.None),
			PhysicalKeycode = (Key)(int)config.GetValue(BindingsSection, $"{actionName}_physical_keycode", (int)Key.None),
			ShiftPressed = (bool)config.GetValue(BindingsSection, $"{actionName}_shift", false),
			CtrlPressed = (bool)config.GetValue(BindingsSection, $"{actionName}_ctrl", false),
			AltPressed = (bool)config.GetValue(BindingsSection, $"{actionName}_alt", false),
			MetaPressed = (bool)config.GetValue(BindingsSection, $"{actionName}_meta", false),
		};
	}

	private void BeginRebind(string actionName)
	{
		_awaitingRebindAction = actionName;
		RefreshBindingLabels();
	}

	private void CancelRebind()
	{
		_awaitingRebindAction = null;
		RefreshBindingLabels();
	}

	private void RebindAction(string actionName, InputEventKey keyEvent)
	{
		InputEventKey newEvent = new()
		{
			Keycode = keyEvent.Keycode,
			PhysicalKeycode = keyEvent.PhysicalKeycode,
			ShiftPressed = keyEvent.ShiftPressed,
			CtrlPressed = keyEvent.CtrlPressed,
			AltPressed = keyEvent.AltPressed,
			MetaPressed = keyEvent.MetaPressed,
		};

		InputMap.ActionEraseEvents(actionName);
		InputMap.ActionAddEvent(actionName, newEvent);
		_awaitingRebindAction = null;
		RefreshBindingLabels();
		SaveSettings();
	}

	private void RefreshBindingLabels()
	{
		bool chinese = UiText.CurrentLocale == UiText.DefaultLocale;
		foreach (RebindRow row in _rebindRows)
		{
			row.NameLabel.Text = chinese ? row.ZhLabel : row.EnLabel;
			row.BindingLabel.Text = _awaitingRebindAction == row.ActionName
				? (chinese ? "按下按键..." : "Press a key...")
				: GetBindingText(row.ActionName);
			row.RebindButton.Text = chinese ? "更换" : "Rebind";
		}

		if (_controlsHint != null)
		{
			_controlsHint.Text = _awaitingRebindAction == null
				? (chinese ? "点击“更换”后按下一个键完成绑定。Esc 关闭设置。" : "Press Rebind, then press a key. Esc closes settings.")
				: (chinese ? "正在等待新的按键，按 Esc 取消。" : "Waiting for a new key. Press Esc to cancel.");
		}
	}

	private string GetBindingText(string actionName)
	{
		InputEventKey? keyEvent = GetFirstKeyEvent(actionName);
		return keyEvent == null ? "-" : FormatKeyEvent(keyEvent);
	}

	private static InputEventKey? GetFirstKeyEvent(string actionName)
	{
		foreach (InputEvent inputEvent in InputMap.ActionGetEvents(actionName))
		{
			if (inputEvent is InputEventKey keyEvent)
			{
				return keyEvent;
			}
		}

		return null;
	}

	private static string FormatKeyEvent(InputEventKey keyEvent)
	{
		List<string> parts = [];
		if (keyEvent.CtrlPressed) parts.Add("Ctrl");
		if (keyEvent.ShiftPressed) parts.Add("Shift");
		if (keyEvent.AltPressed) parts.Add("Alt");
		if (keyEvent.MetaPressed) parts.Add("Meta");

		Key key = keyEvent.PhysicalKeycode != Key.None ? keyEvent.PhysicalKeycode : keyEvent.Keycode;
		parts.Add(key.ToString());
		return string.Join("+", parts);
	}

	private void OnLanguageSelected(long index)
	{
		if (_languageOption == null || _suppressOptionCallbacks)
		{
			return;
		}

		string locale = _languageOption.GetItemMetadata((int)index).AsString();
		UiText.SetLocale(locale);
		SaveSettings();
	}

	private void OnGraphicsModeSelected(long index)
	{
		if (_graphicsModeOption == null || _suppressOptionCallbacks)
		{
			return;
		}

		ApplyWindowMode((int)_graphicsModeOption.GetItemMetadata((int)index));
		SaveSettings();
	}

	private void OnResolutionSelected(long index)
	{
		if (_resolutionOption == null || _suppressOptionCallbacks)
		{
			return;
		}

		string data = _resolutionOption.GetItemMetadata((int)index).AsString();
		string[] parts = data.Split('x');
		if (parts.Length != 2 || !int.TryParse(parts[0], out int width) || !int.TryParse(parts[1], out int height))
		{
			return;
		}

		ApplyResolution(new Vector2I(width, height));
		SaveSettings();
	}

	private void OnVsyncToggled(bool enabled)
	{
		if (_suppressOptionCallbacks)
		{
			return;
		}

		ApplyVsync(enabled);
		SaveSettings();
	}

	private void OnFpsValueChanged(double value)
	{
		if (_suppressOptionCallbacks)
		{
			return;
		}

		ApplyMaxFps((int)value);
		SaveSettings();
	}

	private static void ApplyWindowMode(int modeValue)
	{
		DisplayServer.WindowSetMode((DisplayServer.WindowMode)modeValue);
	}

	private static void ApplyResolution(Vector2I size)
	{
		DisplayServer.WindowSetSize(size);
	}

	private static void ApplyVsync(bool enabled)
	{
		DisplayServer.WindowSetVsyncMode(enabled ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled);
	}

	private static void ApplyMaxFps(int fps)
	{
		Engine.MaxFps = Mathf.Max(0, fps);
	}

	private void ApplyRuntimeSelections()
	{
		_suppressOptionCallbacks = true;

		if (_languageOption != null)
		{
			for (int i = 0; i < _languageOption.ItemCount; i++)
			{
				if (_languageOption.GetItemMetadata(i).AsString() == UiText.CurrentLocale)
				{
					_languageOption.Select(i);
					break;
				}
			}
		}

		if (_graphicsModeOption != null)
		{
			int modeValue = (int)DisplayServer.WindowGetMode();
			for (int i = 0; i < _graphicsModeOption.ItemCount; i++)
			{
				if ((int)_graphicsModeOption.GetItemMetadata(i) == modeValue)
				{
					_graphicsModeOption.Select(i);
					break;
				}
			}
		}

		if (_resolutionOption != null)
		{
			Vector2I currentSize = DisplayServer.WindowGetSize();
			for (int i = 0; i < _resolutionOption.ItemCount; i++)
			{
				string[] parts = _resolutionOption.GetItemMetadata(i).AsString().Split('x');
				if (parts.Length == 2
					&& int.TryParse(parts[0], out int width)
					&& int.TryParse(parts[1], out int height)
					&& currentSize == new Vector2I(width, height))
				{
					_resolutionOption.Select(i);
					break;
				}
			}
		}

		if (_vsyncToggle != null)
		{
			_vsyncToggle.ButtonPressed = DisplayServer.WindowGetVsyncMode() != DisplayServer.VSyncMode.Disabled;
		}

		if (_fpsSpin != null)
		{
			_fpsSpin.Value = Engine.MaxFps;
		}

		_suppressOptionCallbacks = false;
	}

	private void RefreshUiText()
	{
		bool chinese = UiText.CurrentLocale == UiText.DefaultLocale;

		if (_titleLabel != null) _titleLabel.Text = chinese ? "设置" : "Settings";
		if (_tabs != null && _controlsTab != null && _languageTab != null && _graphicsTab != null)
		{
			_tabs.SetTabTitle(_controlsTab.GetIndex(), chinese ? "控制" : "Controls");
			_tabs.SetTabTitle(_languageTab.GetIndex(), chinese ? "语言" : "Language");
			_tabs.SetTabTitle(_graphicsTab.GetIndex(), chinese ? "画面" : "Graphics");
		}

		if (_languageLabel != null) _languageLabel.Text = chinese ? "语言切换" : "Language";
		if (_graphicsLabel != null) _graphicsLabel.Text = chinese ? "显示模式" : "Display Mode";
		if (_resolutionLabel != null) _resolutionLabel.Text = chinese ? "分辨率" : "Resolution";
		if (_vsyncLabel != null) _vsyncLabel.Text = chinese ? "垂直同步" : "VSync";
		if (_fpsLabel != null) _fpsLabel.Text = chinese ? "帧率上限" : "Max FPS";
		if (_vsyncToggle != null) _vsyncToggle.Text = chinese ? "启用" : "Enabled";

		if (_graphicsModeOption != null)
		{
			_graphicsModeOption.SetItemText(0, chinese ? "窗口" : "Windowed");
			_graphicsModeOption.SetItemText(1, chinese ? "全屏" : "Fullscreen");
			_graphicsModeOption.SetItemText(2, chinese ? "独占全屏" : "Exclusive Fullscreen");
		}

		if (_resolutionOption != null)
		{
			for (int i = 0; i < ResolutionOptions.Length && i < _resolutionOption.ItemCount; i++)
			{
				_resolutionOption.SetItemText(i, chinese ? ResolutionOptions[i].ZhLabel : ResolutionOptions[i].EnLabel);
			}
		}

		RefreshBindingLabels();
		ApplyRuntimeSelections();
	}

	private void OnLanguageChanged()
	{
		RefreshUiText();
	}
}
