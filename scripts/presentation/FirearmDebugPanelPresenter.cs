using Godot;
using BattleHarvesterStudy.Combat.Firearms;
using BattleHarvesterStudy.Equipment;
using BattleHarvesterStudy.Inventory;
using BattleHarvesterStudy.Items;

namespace BattleHarvesterStudy.Presentation;

public partial class FirearmDebugPanelPresenter : Node
{
	private const string ToggleAction = "toggle_firearm_debug";

	[Export] public NodePath UiContextPath { get; set; } = new("../PlayerUiContext");
	[Export] public NodePath PanelPath { get; set; } = new("../../FirearmDebugPanel");
	[Export] public NodePath TitleLabelPath { get; set; } = new("../../FirearmDebugPanel/Margin/VBox/HeaderBar/Title");
	[Export] public NodePath CloseButtonPath { get; set; } = new("../../FirearmDebugPanel/Margin/VBox/HeaderBar/CloseButton");
	[Export] public NodePath SummaryLabelPath { get; set; } = new("../../FirearmDebugPanel/Margin/VBox/Summary");

	private PlayerUiContext? _uiContext;
	private PanelContainer? _panel;
	private Label? _titleLabel;
	private Button? _closeButton;
	private Label? _summaryLabel;

	public bool IsOpen => _panel?.Visible ?? false;

	public override void _Ready()
	{
		_uiContext = GetNodeOrNull<PlayerUiContext>(UiContextPath);
		_panel = GetNodeOrNull<PanelContainer>(PanelPath);
		_titleLabel = GetNodeOrNull<Label>(TitleLabelPath);
		_closeButton = GetNodeOrNull<Button>(CloseButtonPath);
		_summaryLabel = GetNodeOrNull<Label>(SummaryLabelPath);

		if (_panel != null)
		{
			_panel.Visible = false;
		}

		if (_closeButton != null)
		{
			_closeButton.Pressed += ClosePanel;
		}

		SetProcess(true);
		UiText.LanguageChanged += RefreshText;
		RefreshText();
	}

	public override void _ExitTree()
	{
		if (_closeButton != null)
		{
			_closeButton.Pressed -= ClosePanel;
		}

		UiText.LanguageChanged -= RefreshText;
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed(ToggleAction))
		{
			TogglePanel();
		}

		if (IsOpen)
		{
			RefreshText();
		}
	}

	private void TogglePanel()
	{
		if (_panel == null)
		{
			return;
		}

		_panel.Visible = !_panel.Visible;
		if (_panel.Visible)
		{
			RefreshText();
		}
	}

	private void ClosePanel()
	{
		if (_panel != null)
		{
			_panel.Visible = false;
		}
	}

	private void RefreshText()
	{
		bool chinese = UiText.CurrentLocale == UiText.DefaultLocale;
		if (_titleLabel != null)
		{
			_titleLabel.Text = chinese ? "枪械调试" : "Firearm Debug";
		}

		if (_summaryLabel == null)
		{
			return;
		}

		FirearmCombatComponent? firearmCombat = _uiContext?.FirearmCombat;
		EquipmentComponent? equipment = _uiContext?.Equipment;
		InventoryComponent? inventory = _uiContext?.Inventory;
		FirearmWeaponDefinition? firearm = equipment?.GetActiveFirearmDefinition();
		ItemInstance? firearmItem = equipment?.GetActiveWeaponItem();
		if (firearmCombat == null || firearm == null || firearmItem == null)
		{
			_summaryLabel.Text = chinese ? "当前没有装备枪械。" : "No firearm equipped.";
			return;
		}

		FirearmResolvedStats resolved = FirearmStatResolver.Resolve(firearm, firearmItem);
		int reserveAmmo = FirearmAmmoInventoryService.GetReserveAmmoCount(inventory, resolved.AmmoType);
		string loadedAmmoName = firearmItem.CurrentMagazineAmmo > 0
			? FirearmTextFormatter.GetAmmoDisplayName(firearmItem.GetLoadedAmmoDefinition(), chinese)
			: (reserveAmmo > 0
				? FirearmTextFormatter.GetAmmoTypeName(resolved.AmmoType, chinese)
				: (chinese ? "无弹药" : "No Ammo"));

		string rangeBand = firearmCombat.CurrentRangeBand switch
		{
			FirearmRangeBand.Effective => chinese ? "有效" : "Effective",
			FirearmRangeBand.Falloff => chinese ? "衰减" : "Falloff",
			FirearmRangeBand.Severe => chinese ? "重衰减" : "Severe",
			_ => chinese ? "无目标" : "No Target"
		};

		string targetName = firearmCombat.CurrentTarget == null
			? (chinese ? "无" : "None")
			: firearmCombat.CurrentTarget.Name;

		_summaryLabel.Text = string.Join("\n", new[]
		{
			$"{GetLabel(chinese, "武器", "Weapon")}  {ContentTextFormatter.GetItemDisplayName(firearmItem.Definition)}",
			$"{GetLabel(chinese, "目标", "Target")}  {targetName}",
			$"{GetLabel(chinese, "命中率", "Hit Chance")}  {firearmCombat.CurrentHitChance:0.#}%",
			$"{GetLabel(chinese, "直接值", "Direct Hit")}  {firearmCombat.CurrentDirectHitChance:0.#}",
			$"{GetLabel(chinese, "基础命中", "Base Hit")}  {firearmCombat.CurrentBaseHitComponent:0.#}",
			$"{GetLabel(chinese, "瞄准基础", "Aim Base")}  {firearmCombat.CurrentAimBaseBonusComponent:0.#}",
			$"{GetLabel(chinese, "精度加成", "Precision Bonus")}  {firearmCombat.CurrentAimPrecisionBonusComponent:0.#}",
			$"{GetLabel(chinese, "腰射加成", "Hip Fire Bonus")}  {firearmCombat.CurrentHipFireBonusComponent:0.#}",
			$"{GetLabel(chinese, "自身移动惩罚", "Self Move Penalty")}  -{firearmCombat.CurrentSelfMovingPenaltyComponent:0.#}",
			$"{GetLabel(chinese, "目标移动惩罚", "Target Move Penalty")}  -{firearmCombat.CurrentTargetMovingPenaltyComponent:0.#}",
			$"{GetLabel(chinese, "射程惩罚", "Range Penalty")}  -{firearmCombat.CurrentRangePenaltyComponent:0.#}",
			$"{GetLabel(chinese, "后坐控制", "Recoil Control")}  {resolved.RecoilControl:0.#}",
			$"{GetLabel(chinese, "后坐倍率", "Recoil Mult")}  x{firearmCombat.CurrentRecoilMultiplier:0.###}",
			$"{GetLabel(chinese, "射速", "Fire Rate")}  {resolved.FireRate:0.##}/s",
			$"{GetLabel(chinese, "锥形角度", "Cone Angle")}  {firearmCombat.CurrentConeAngleDegrees:0.#}°",
			$"{GetLabel(chinese, "距离", "Distance")}  {firearmCombat.CurrentTargetDistance:0.0}",
			$"{GetLabel(chinese, "区间", "Range Band")}  {rangeBand}",
			$"{GetLabel(chinese, "伤害倍率", "Damage Mult")}  x{firearmCombat.CurrentRangeDamageMultiplier:0.##}",
			$"{GetLabel(chinese, "弹药", "Ammo")}  {loadedAmmoName}",
			$"{GetLabel(chinese, "弹药PT", "Ammo PT")}  {firearmItem.LoadedAmmoPenetrationTier:0.##}",
			$"{GetLabel(chinese, "弹匣", "Magazine")}  {firearmItem.CurrentMagazineAmmo}/{resolved.MagazineCapacity}",
			$"{GetLabel(chinese, "备弹", "Reserve")}  {reserveAmmo}",
			$"{GetLabel(chinese, "瞄准", "Aim")}  {(firearmCombat.IsAiming ? GetLabel(chinese, "开启", "On") : GetLabel(chinese, "关闭", "Off"))}",
			$"{GetLabel(chinese, "自身移动", "Owner Moving")}  {(firearmCombat.CurrentOwnerMoving ? GetLabel(chinese, "是", "Yes") : GetLabel(chinese, "否", "No"))}",
			$"{GetLabel(chinese, "目标移动", "Target Moving")}  {(firearmCombat.CurrentTargetMoving ? GetLabel(chinese, "是", "Yes") : GetLabel(chinese, "否", "No"))}"
		});
	}

	private static string GetLabel(bool chinese, string zh, string en)
	{
		return chinese ? zh : en;
	}
}
