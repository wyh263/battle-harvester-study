using Godot;
using BattleHarvesterStudy.Combat.Firearms;
using BattleHarvesterStudy.Equipment;
using BattleHarvesterStudy.Items;

namespace BattleHarvesterStudy.Presentation;

public partial class FirearmWeaponHudPresenter : Node
{
	[Export] public NodePath UiContextPath { get; set; } = new("../PlayerUiContext");

	private PlayerUiContext? _uiContext;
	private PanelContainer? _panel;
	private Label? _titleLabel;
	private Label? _bodyLabel;

	public override void _Ready()
	{
		_uiContext = GetNodeOrNull<PlayerUiContext>(UiContextPath);
		EnsurePanel();
		UiText.LanguageChanged += Refresh;
		SetProcess(true);
		Refresh();
	}

	public override void _ExitTree()
	{
		UiText.LanguageChanged -= Refresh;
	}

	public override void _Process(double delta)
	{
		Refresh();
	}

	private void EnsurePanel()
	{
		CanvasLayer? root = GetOwner<CanvasLayer>();
		if (root == null)
		{
			return;
		}

		_panel = root.GetNodeOrNull<PanelContainer>("BottomLeftWeaponPanel");
		if (_panel != null)
		{
			_titleLabel = _panel.GetNodeOrNull<Label>("Margin/VBox/Title");
			_bodyLabel = _panel.GetNodeOrNull<Label>("Margin/VBox/Body");
			return;
		}

		_panel = new PanelContainer
		{
			Name = "BottomLeftWeaponPanel",
			OffsetLeft = 20.0f,
			OffsetTop = 0.0f,
			OffsetRight = 320.0f,
			OffsetBottom = 0.0f
		};
		_panel.SetAnchorsPreset(Control.LayoutPreset.BottomLeft);
		_panel.OffsetTop = -158.0f;
		_panel.OffsetBottom = -20.0f;

		MarginContainer margin = new() { Name = "Margin" };
		margin.AddThemeConstantOverride("margin_left", 12);
		margin.AddThemeConstantOverride("margin_top", 10);
		margin.AddThemeConstantOverride("margin_right", 12);
		margin.AddThemeConstantOverride("margin_bottom", 10);
		_panel.AddChild(margin);

		VBoxContainer vBox = new() { Name = "VBox" };
		vBox.AddThemeConstantOverride("separation", 6);
		margin.AddChild(vBox);

		_titleLabel = new Label
		{
			Name = "Title",
			HorizontalAlignment = HorizontalAlignment.Center
		};
		vBox.AddChild(_titleLabel);

		_bodyLabel = new Label
		{
			Name = "Body",
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};
		vBox.AddChild(_bodyLabel);

		root.AddChild(_panel);
	}

	private void Refresh()
	{
		if (_panel == null || _titleLabel == null || _bodyLabel == null)
		{
			return;
		}

		bool chinese = UiText.CurrentLocale == UiText.DefaultLocale;
		_titleLabel.Text = chinese ? "\u5f53\u524d\u6b66\u5668" : "Current Weapon";

		EquipmentComponent? equipment = _uiContext?.Equipment;
		InventoryComponent? inventory = _uiContext?.Inventory;
		FirearmWeaponDefinition? firearm = equipment?.GetActiveFirearmDefinition();
		ItemInstance? firearmItem = equipment?.GetActiveWeaponItem();
		if (firearm == null || firearmItem == null)
		{
			_panel.Visible = false;
			return;
		}

		_panel.Visible = true;
		FirearmResolvedStats resolved = FirearmStatResolver.Resolve(firearm, firearmItem);
		AmmoItemDefinition? loadedAmmo = firearmItem.GetLoadedAmmoDefinition();
		int reserveAmmo = FirearmAmmoInventoryService.GetReserveAmmoCount(inventory, resolved.AmmoType);
		string ammoDisplayName = firearmItem.CurrentMagazineAmmo > 0
			? FirearmTextFormatter.GetAmmoDisplayName(loadedAmmo, chinese)
			: (reserveAmmo > 0
				? FirearmTextFormatter.GetAmmoTypeName(resolved.AmmoType, chinese)
				: (chinese ? "\u65e0\u5f39\u836f" : "No Ammo"));

		_bodyLabel.Text = string.Join("\n", new[]
		{
			$"{GetLabel(chinese, "\u6b66\u5668", "Weapon")}  {ContentTextFormatter.GetItemDisplayName(firearmItem.Definition)}",
			$"{GetLabel(chinese, "\u5f39\u5323", "Magazine")}  {firearmItem.CurrentMagazineAmmo}/{resolved.MagazineCapacity}",
			$"{GetLabel(chinese, "\u5907\u5f39", "Reserve")}  {reserveAmmo}",
			$"{GetLabel(chinese, "\u5f39\u836f", "Ammo")}  {ammoDisplayName}",
			$"{GetLabel(chinese, "PT", "PT")}  {firearmItem.LoadedAmmoPenetrationTier:0.##}",
			$"{GetLabel(chinese, "\u5c04\u51fb\u6a21\u5f0f", "Fire Mode")}  {FirearmTextFormatter.GetFireModeName(resolved.FireMode, chinese)}"
		});
	}

	private static string GetLabel(bool chinese, string zh, string en)
	{
		return chinese ? zh : en;
	}
}
