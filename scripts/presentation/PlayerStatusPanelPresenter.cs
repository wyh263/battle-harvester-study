using Godot;
using BattleHarvesterStudy.Attributes;
using BattleHarvesterStudy.Combat;
using BattleHarvesterStudy.Combat.Firearms;
using BattleHarvesterStudy.Equipment;
using BattleHarvesterStudy.Inventory;
using BattleHarvesterStudy.Inventory.Search;
using BattleHarvesterStudy.Items;

namespace BattleHarvesterStudy.Presentation;

public partial class PlayerStatusPanelPresenter : Node
{
	[Export] public NodePath UiContextPath { get; set; } = new("../PlayerUiContext");
	[Export] public NodePath HeaderLabelPath { get; set; } = new("../../InventoryUi/PlayerInventoryWindow/Margin/VBox/ContentRow/StatusColumn/StatusPanel/Margin/VBox/Header");
	[Export] public NodePath SummaryLabelPath { get; set; } = new("../../InventoryUi/PlayerInventoryWindow/Margin/VBox/ContentRow/StatusColumn/StatusPanel/Margin/VBox/Summary");

	private Label? _headerLabel;
	private Label? _summaryLabel;
	private PlayerUiContext? _uiContext;

	public override void _Ready()
	{
		_uiContext = GetNodeOrNull<PlayerUiContext>(UiContextPath);
		_headerLabel = GetNodeOrNull<Label>(HeaderLabelPath);
		_summaryLabel = GetNodeOrNull<Label>(SummaryLabelPath);
	}

	public void Present()
	{
		bool chinese = UiText.CurrentLocale == UiText.DefaultLocale;
		if (_headerLabel != null)
		{
			_headerLabel.Text = UiText.Resolve(UiTextKeys.Status.PlayerHeader);
		}

		if (_summaryLabel == null)
		{
			return;
		}

		StatsComponent? stats = _uiContext?.ResolveStats();
		HealthComponent? health = _uiContext?.PlayerHealth;
		ArmorComponent? armor = _uiContext?.PlayerArmor;
		ActorSkillResourceController? resources = _uiContext?.SkillResources;
		EquipmentComponent? equipment = _uiContext?.Equipment;
		ActiveContainerModifierComponent? activeContainerModifiers = _uiContext?.ActiveContainerModifiers;
		FirearmCombatComponent? firearmCombat = _uiContext?.FirearmCombat;
		FirearmWeaponDefinition? firearm = equipment?.GetActiveFirearmDefinition();
		ItemInstance? firearmItem = equipment?.GetActiveWeaponItem();
		if (stats == null || health == null)
		{
			_summaryLabel.Text = "-";
			return;
		}

		FirearmResolvedStats? resolvedFirearm = firearm == null
			? null
			: FirearmStatResolver.Resolve(firearm, firearmItem);
		string firearmBlock = firearm == null || resolvedFirearm == null
			? $"{Label(UiTextKeys.Status.Firearm)}  -"
			: string.Join("\n", new[]
			{
				$"{Label(UiTextKeys.Status.Firearm)}  {WeaponClassificationUtility.GetFamilyName(firearm.WeaponFamilyId, chinese)}",
				$"{Label(UiTextKeys.Status.Hit)}  {firearmCombat?.CurrentHitChance ?? 0.0f:0.#}%",
				$"{Label(UiTextKeys.Status.Magazine)}  {firearmItem?.CurrentMagazineAmmo ?? 0}/{resolvedFirearm.MagazineCapacity}",
				$"{Label(UiTextKeys.Status.Aim)}  {(firearmCombat?.IsAiming == true ? Label(UiTextKeys.Status.On) : Label(UiTextKeys.Status.Off))}",
				$"{Label(UiTextKeys.Status.FireMode)}  {FormatFireMode(resolvedFirearm.FireMode)}"
			});

		_summaryLabel.Text =
			$"{Label(UiTextKeys.Status.Health)}  {health.CurrentHealth:0}/{health.MaxHealth:0}\n" +
			$"AP  {armor?.CurrentArmor ?? 0:0}/{armor?.MaxArmor ?? 0:0}\n" +
			$"{Label(UiTextKeys.Status.Stamina)}  {resources?.CurrentResource ?? 0:0}/{resources?.MaxResource ?? 0:0}\n" +
			$"{Label(UiTextKeys.Status.Attack)}  {stats.GetValue(StatType.AttackPower):0}\n" +
			$"PT  {stats.GetValue(StatType.WeaponPenetrationTier):0.##}\n" +
			$"{Label(UiTextKeys.Status.ActiveWeapon)}  {EquipmentTextFormatter.GetSlotDisplayName(equipment?.ActiveWeaponSlot ?? EquipmentSlotType.None)}\n" +
			$"{Label(UiTextKeys.Status.Defense)}  {stats.GetValue(StatType.Defense):0}\n" +
			$"Prof  {stats.GetValue(StatType.CombatProficiency):0.##} / PT+{Mathf.Clamp(stats.GetValue(StatType.CombatProficiency) * 0.01f, 0.0f, 1.0f):0.##}\n" +
			$"AT  {stats.GetValue(StatType.ArmorTier):0.##} / Absorb {stats.GetValue(StatType.ArmorAbsorbRate) * 100.0f:0.#}%\n" +
			$"K  {stats.GetValue(StatType.DefenseReductionK):0}\n" +
			$"{Label(UiTextKeys.Status.MoveSpeed)}  {stats.GetValue(StatType.MoveSpeed):0.0}\n" +
			$"{firearmBlock}\n" +
			$"{Label(UiTextKeys.Status.SearchMultiplier)}  {activeContainerModifiers?.GetGlobalSearchSpeedMultiplier() ?? 1.0f:0.##}x\n" +
			$"{Label(UiTextKeys.Status.HighRarity)}  {FormatRarityBias(activeContainerModifiers)}\n" +
			$"{Label(UiTextKeys.Status.ActiveSearchMods)}  {activeContainerModifiers?.ActiveModifierCount ?? 0}";
	}

	private static string FormatRarityBias(ActiveContainerModifierComponent? modifiers)
	{
		float blue = modifiers?.GetGlobalLootRarityWeightMultiplier(LootRarity.Blue) ?? 1.0f;
		float purple = modifiers?.GetGlobalLootRarityWeightMultiplier(LootRarity.Purple) ?? 1.0f;
		float gold = modifiers?.GetGlobalLootRarityWeightMultiplier(LootRarity.Gold) ?? 1.0f;
		float red = modifiers?.GetGlobalLootRarityWeightMultiplier(LootRarity.Red) ?? 1.0f;
		return $"B {blue:0.##}x  P {purple:0.##}x  G {gold:0.##}x  R {red:0.##}x";
	}

	private static string FormatFireMode(FirearmFireMode fireMode)
	{
		return fireMode switch
		{
			FirearmFireMode.Automatic => Label(UiTextKeys.Status.FireModeAutomatic),
			FirearmFireMode.Selective => Label(UiTextKeys.Status.FireModeSelective),
			_ => Label(UiTextKeys.Status.FireModeSingleShot)
		};
	}

	private static string Label(string key)
	{
		return UiText.Resolve(key);
	}
}
