using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using RuntimeDictionary = System.Collections.Generic.Dictionary<string, string>;
using RuntimeObjectDictionary = System.Collections.Generic.Dictionary<string, object?>;

namespace BattleHarvesterStudy.Presentation;

public static class UiText
{
	public const string DefaultLocale = "zh-Hans";
	private const string FallbackLocale = "en";

	private static readonly Dictionary<string, RuntimeDictionary> Catalog = new()
	{
		[DefaultLocale] = new RuntimeDictionary
		{
			[UiTextKeys.Hud.Health] = "生命  {current}/{max}",
			[UiTextKeys.Hud.HealthDead] = "生命  已死亡",
			[UiTextKeys.Hud.Armor] = "护甲  {current}/{max}",
			[UiTextKeys.Hud.Target] = "目标  {name}",
			[UiTextKeys.Hud.TargetFree] = "目标  自由",
			[UiTextKeys.Hud.TargetNone] = "无目标",
			[UiTextKeys.Hud.TargetDead] = "{name}  已死亡",
			[UiTextKeys.Hud.Ready] = "就绪",
			[UiTextKeys.Hud.Seconds] = "{seconds}秒",
			[UiTextKeys.Hud.SkillSlot] = "{slot}\n{skill}\n{state}",
			[UiTextKeys.Hud.Resource] = "{label}  {current}/{max}",

			[UiTextKeys.Inventory.HeaderPlayer] = "角色背包",
			[UiTextKeys.Inventory.HeaderContainer] = "容器",
			[UiTextKeys.Inventory.HeaderEquipment] = "装备",
			[UiTextKeys.Inventory.SummaryUnconfigured] = "角色背包\n未配置",
			[UiTextKeys.Inventory.SummaryClosed] = "容器已关闭",
			[UiTextKeys.Inventory.SelectedEmpty] = "已选中  空格",
			[UiTextKeys.Inventory.SelectedItem] = "已选中  {name} x{count}",
			[UiTextKeys.Inventory.SelectedUnsearched] = "当前选中：未搜索物品",
			[UiTextKeys.Inventory.RunLoot] = "本局收获  {value}",
			[UiTextKeys.Inventory.PlayerSummary] = "{title}\n物品  {item_count}\n空间  {used_cells}/{total_cells}\n{selected}\n{toggle_hint}\n{take_all_hint}\n{quick_transfer_hint}",
			[UiTextKeys.Inventory.ContainerSummary] = "{title}\n物品  {item_count}\n{selected}\n{drag_hint}\n{rotate_hint}\n{quick_transfer_hint}\n{close_hint}",
			[UiTextKeys.Inventory.WarehouseModeInactive] = "仓库模式\n点击“出售”开始勾选",
			[UiTextKeys.Inventory.WarehouseModeActive] = "出售模式\n已选 {count} 件\n预计获得 {value}",
			[UiTextKeys.Inventory.SellButton] = "出售",
			[UiTextKeys.Inventory.StopSellingButton] = "结束出售",
			[UiTextKeys.Inventory.SellSelectedButton] = "一键出售 {count} 件  +{value}",
			[UiTextKeys.Inventory.EquipmentSummary] = "{equipped_count}/{slot_count} 已装备\n{open_hint}\n{drag_hint}",
			[UiTextKeys.Inventory.DetailsEmpty] = "物品详情\n未选中物品",
			[UiTextKeys.Inventory.DetailsTitle] = "物品详情",
			[UiTextKeys.Inventory.DetailsBody] = "{title}\n名称  {name}\n类别  {category}\n堆叠  {stack}/{max_stack}\n尺寸  {width}x{height}\n旋转  {rotated}\n价值  {value}\n标签  {tags}\n{equipment_block}",
			[UiTextKeys.Inventory.DetailsEquipmentNone] = "装备信息\n不可装备",
			[UiTextKeys.Inventory.DetailsEquipmentBlock] = "装备信息\n样式  {archetype}\n强度  {item_power}\n可用槽位  {slots}\n双手占用  {two_handed}\n当前修正\n{modifiers}\n可滚区间\n{roll_bands}",
			[UiTextKeys.Inventory.DetailsUsableNone] = "使用信息\n不可使用",
			[UiTextKeys.Inventory.DetailsUsableBlock] = "使用信息\n生命  +{health}\n资源  +{resource}\n使用后消耗  {consume_on_use}\n满值可用  {allow_use_at_full}",
			[UiTextKeys.Inventory.StatusCannotAccess] = "无法访问",
			[UiTextKeys.Inventory.StatusOpenedContainer] = "已打开  {name}",
			[UiTextKeys.Inventory.StatusClosedContainer] = "已关闭  {name}",
			[UiTextKeys.Inventory.StatusOpenedInventory] = "背包已打开",
			[UiTextKeys.Inventory.StatusClosedInventory] = "背包已关闭",
			[UiTextKeys.Inventory.StatusQuickTransferSuccess] = "快速转移成功  {name}",
			[UiTextKeys.Inventory.StatusQuickTransferFailure] = "快速转移失败  {name}",
			[UiTextKeys.Inventory.StatusTakeAll] = "全部拿取  成功 {moved}  失败 {failed}",
			[UiTextKeys.Inventory.StatusMoveSuccess] = "移动成功  {name}",
			[UiTextKeys.Inventory.StatusMoveFailure] = "放置失败  {name}",
			[UiTextKeys.Inventory.StatusEquipSuccess] = "已装备  {name} -> {slot}",
			[UiTextKeys.Inventory.StatusEquipFailure] = "装备失败  {name} -> {slot}  {reason}",
			[UiTextKeys.Inventory.StatusUnequipSuccess] = "已卸下  {name} <- {slot}",
			[UiTextKeys.Inventory.StatusUnequipFailure] = "卸下失败  {name} <- {slot}  {reason}",
			[UiTextKeys.Inventory.StatusUseSuccess] = "已使用  {name}",
			[UiTextKeys.Inventory.StatusUseFailure] = "使用失败  {name}  {reason}",
			[UiTextKeys.Inventory.StatusSoldItems] = "已出售  {count} 件  获得 {value}",
			[UiTextKeys.Inventory.StatusWeaponSkillEquipped] = "武器技能已装入  {name} -> 槽位 {slot}",
			[UiTextKeys.Inventory.StatusWeaponSkillEquipFailed] = "武器技能装入失败  槽位 {slot}",
			[UiTextKeys.Inventory.StatusWeaponSkillRemoved] = "武器技能已卸下  {name} <- 槽位 {slot}",
			[UiTextKeys.Inventory.StatusSecureInsurancePurchased] = "{type}已生效  花费 {value}",
			[UiTextKeys.Inventory.StatusSecureInsuranceFailed] = "{type}购买失败  {reason}",
			[UiTextKeys.Inventory.StatusSecureStoreFailed] = "安全箱存入失败  {name}  {reason}",
			[UiTextKeys.Inventory.SecureContainerHeader] = "安全箱",
			[UiTextKeys.Inventory.SecureInsuranceUninitialized] = "保险状态  未初始化",
			[UiTextKeys.Inventory.SecureInsuranceRentalActive] = "保险状态  租用中\n剩余局数 {runs}\n待缴账单 {bill}\n当前贪婪 {greed}%\n未使用局数 {unused_runs}\n挑战额度 {quota}",
			[UiTextKeys.Inventory.SecureInsuranceChallengeActive] = "保险状态  挑战资格\n剩余额度 {quota}\n待缴账单 {bill}\n当前贪婪 {greed}%\n未使用局数 {unused_runs}",
			[UiTextKeys.Inventory.SecureInsuranceRetrievalOnly] = "保险状态  仅可提取\n待缴账单 {bill}\n当前贪婪 {greed}%",
			[UiTextKeys.Inventory.SecureInsuranceNone] = "保险状态  无资格",
			[UiTextKeys.Inventory.SecureQuotaButton] = "额度由挑战获取",
			[UiTextKeys.Inventory.SecureRentalButton] = "购买租用资格 +{runs}局 -{cost}",
			[UiTextKeys.Inventory.SecureRentalExtendButton] = "续租 +{runs}局 -{cost}",
			[UiTextKeys.Inventory.SecureRentalType] = "租用资格",
			[UiTextKeys.Inventory.InsufficientCredits] = "资金不足",
			[UiTextKeys.Inventory.DragHint] = "{name}  x{count}",
			[UiTextKeys.Inventory.HintToggleInventory] = "B 打开/关闭背包",
			[UiTextKeys.Inventory.HintTakeAll] = "F 全部拿取",
			[UiTextKeys.Inventory.HintQuickTransfer] = "Ctrl+左键 快速转移",
			[UiTextKeys.Inventory.HintDragItem] = "左键拖拽物品",
			[UiTextKeys.Inventory.HintRotateDragging] = "拖拽时右键旋转",
			[UiTextKeys.Inventory.HintCloseContainer] = "Q 关闭容器",
			[UiTextKeys.Inventory.HintInspectSlot] = "双击槽位查看详情",
			[UiTextKeys.Inventory.HintDragToEquip] = "拖拽物品到槽位进行装备",
			[UiTextKeys.Inventory.HintUseItem] = "右键使用物品",
			[UiTextKeys.Inventory.AccessMissingRequester] = "访问需要有效的请求者",
			[UiTextKeys.Inventory.AccessLocked] = "容器已上锁",
			[UiTextKeys.Inventory.AccessSingleUseConsumed] = "容器访问次数已耗尽",
			[UiTextKeys.Inventory.AccessOutOfRange] = "请靠近一点 ({current}/{required})",
			[UiTextKeys.Inventory.AccessMissingTag] = "缺少访问标签 {tag}",

			[UiTextKeys.World.ContainerUnavailable] = "不可访问",
			[UiTextKeys.World.ContainerLocked] = "已上锁",
			[UiTextKeys.World.ContainerConsumed] = "已使用",
			[UiTextKeys.World.ContainerOpenable] = "可打开",
			[UiTextKeys.World.ContainerLabel] = "箱子\n{access}\n物品 {item_count}",

			[UiTextKeys.Status.PlayerHeader] = "玩家状态",
			[UiTextKeys.Status.Firearm] = "枪械",
			[UiTextKeys.Status.Hit] = "命中",
			[UiTextKeys.Status.Magazine] = "弹匣",
			[UiTextKeys.Status.Aim] = "瞄准",
			[UiTextKeys.Status.On] = "开启",
			[UiTextKeys.Status.Off] = "关闭",
			[UiTextKeys.Status.FireMode] = "射击模式",
			[UiTextKeys.Status.FireModeAutomatic] = "全自动",
			[UiTextKeys.Status.FireModeSelective] = "单点+全自动",
			[UiTextKeys.Status.FireModeSingleShot] = "单发",
			[UiTextKeys.Status.Health] = "生命",
			[UiTextKeys.Status.Stamina] = "精力",
			[UiTextKeys.Status.Attack] = "攻击",
			[UiTextKeys.Status.ActiveWeapon] = "当前武器",
			[UiTextKeys.Status.Defense] = "防御",
			[UiTextKeys.Status.MoveSpeed] = "移动速度",
			[UiTextKeys.Status.SearchMultiplier] = "搜索倍率",
			[UiTextKeys.Status.HighRarity] = "高阶爆率",
			[UiTextKeys.Status.ActiveSearchMods] = "已激活搜索效果",
		},
		[FallbackLocale] = new RuntimeDictionary
		{
			[UiTextKeys.Hud.Health] = "Health  {current}/{max}",
			[UiTextKeys.Hud.HealthDead] = "Health  Dead",
			[UiTextKeys.Hud.Armor] = "Armor  {current}/{max}",
			[UiTextKeys.Hud.Target] = "Target  {name}",
			[UiTextKeys.Hud.TargetFree] = "Target  Free",
			[UiTextKeys.Hud.TargetNone] = "No Target",
			[UiTextKeys.Hud.TargetDead] = "{name}  Dead",
			[UiTextKeys.Hud.Ready] = "Ready",
			[UiTextKeys.Hud.Seconds] = "{seconds}s",
			[UiTextKeys.Hud.SkillSlot] = "{slot}\n{skill}\n{state}",
			[UiTextKeys.Hud.Resource] = "{label}  {current}/{max}",

			[UiTextKeys.Inventory.HeaderPlayer] = "Player Inventory",
			[UiTextKeys.Inventory.HeaderContainer] = "Container",
			[UiTextKeys.Inventory.HeaderEquipment] = "Equipment",
			[UiTextKeys.Inventory.SummaryUnconfigured] = "Player Inventory\nUnconfigured",
			[UiTextKeys.Inventory.SummaryClosed] = "Container Closed",
			[UiTextKeys.Inventory.SelectedEmpty] = "Selected  Empty Cell",
			[UiTextKeys.Inventory.SelectedItem] = "Selected  {name} x{count}",
			[UiTextKeys.Inventory.SelectedUnsearched] = "Selected: Unsearched Item",
			[UiTextKeys.Inventory.RunLoot] = "Run Loot  {value}",
			[UiTextKeys.Inventory.PlayerSummary] = "{title}\nItems  {item_count}\nSpace  {used_cells}/{total_cells}\n{selected}\n{toggle_hint}\n{take_all_hint}\n{quick_transfer_hint}",
			[UiTextKeys.Inventory.ContainerSummary] = "{title}\nItems  {item_count}\n{selected}\n{drag_hint}\n{rotate_hint}\n{quick_transfer_hint}\n{close_hint}",
			[UiTextKeys.Inventory.WarehouseModeInactive] = "Warehouse Mode\nPress Sell to start selecting",
			[UiTextKeys.Inventory.WarehouseModeActive] = "Sell Mode\nSelected {count}\nValue {value}",
			[UiTextKeys.Inventory.SellButton] = "Sell",
			[UiTextKeys.Inventory.StopSellingButton] = "Stop Selling",
			[UiTextKeys.Inventory.SellSelectedButton] = "Sell Selected {count}  +{value}",
			[UiTextKeys.Inventory.EquipmentSummary] = "{equipped_count}/{slot_count} equipped\n{open_hint}\n{drag_hint}",
			[UiTextKeys.Inventory.DetailsEmpty] = "Item Details\nNo Item Selected",
			[UiTextKeys.Inventory.DetailsTitle] = "Item Details",
			[UiTextKeys.Inventory.DetailsBody] = "{title}\nName  {name}\nCategory  {category}\nStack  {stack}/{max_stack}\nSize  {width}x{height}\nRotated  {rotated}\nValue  {value}\nTags  {tags}\n{equipment_block}",
			[UiTextKeys.Inventory.DetailsEquipmentNone] = "Equipment Info\nNot Equippable",
			[UiTextKeys.Inventory.DetailsEquipmentBlock] = "Equipment Info\nArchetype  {archetype}\nPower  {item_power}\nAllowed Slots  {slots}\nTwo-Handed  {two_handed}\nCurrent Modifiers\n{modifiers}\nRoll Bands\n{roll_bands}",
			[UiTextKeys.Inventory.DetailsUsableNone] = "Use Info\nNot Usable",
			[UiTextKeys.Inventory.DetailsUsableBlock] = "Use Info\nHealth  +{health}\nResource  +{resource}\nConsumes On Use  {consume_on_use}\nAllow At Full  {allow_use_at_full}",
			[UiTextKeys.Inventory.StatusCannotAccess] = "Cannot Access",
			[UiTextKeys.Inventory.StatusOpenedContainer] = "Opened  {name}",
			[UiTextKeys.Inventory.StatusClosedContainer] = "Closed  {name}",
			[UiTextKeys.Inventory.StatusOpenedInventory] = "Inventory Opened",
			[UiTextKeys.Inventory.StatusClosedInventory] = "Inventory Closed",
			[UiTextKeys.Inventory.StatusQuickTransferSuccess] = "Quick Transfer Success  {name}",
			[UiTextKeys.Inventory.StatusQuickTransferFailure] = "Quick Transfer Failed  {name}",
			[UiTextKeys.Inventory.StatusTakeAll] = "Take All  Success {moved}  Failed {failed}",
			[UiTextKeys.Inventory.StatusMoveSuccess] = "Moved  {name}",
			[UiTextKeys.Inventory.StatusMoveFailure] = "Placement Failed  {name}",
			[UiTextKeys.Inventory.StatusEquipSuccess] = "Equipped  {name} -> {slot}",
			[UiTextKeys.Inventory.StatusEquipFailure] = "Equip Failed  {name} -> {slot}  {reason}",
			[UiTextKeys.Inventory.StatusUnequipSuccess] = "Unequipped  {name} <- {slot}",
			[UiTextKeys.Inventory.StatusUnequipFailure] = "Unequip Failed  {name} <- {slot}  {reason}",
			[UiTextKeys.Inventory.StatusUseSuccess] = "Used  {name}",
			[UiTextKeys.Inventory.StatusUseFailure] = "Use Failed  {name}  {reason}",
			[UiTextKeys.Inventory.StatusSoldItems] = "Sold  {count} items  +{value}",
			[UiTextKeys.Inventory.StatusWeaponSkillEquipped] = "Weapon Skill Equipped  {name} -> Slot {slot}",
			[UiTextKeys.Inventory.StatusWeaponSkillEquipFailed] = "Weapon Skill Equip Failed  Slot {slot}",
			[UiTextKeys.Inventory.StatusWeaponSkillRemoved] = "Weapon Skill Removed  {name} <- Slot {slot}",
			[UiTextKeys.Inventory.StatusSecureInsurancePurchased] = "{type} activated  cost {value}",
			[UiTextKeys.Inventory.StatusSecureInsuranceFailed] = "{type} purchase failed  {reason}",
			[UiTextKeys.Inventory.StatusSecureStoreFailed] = "Secure storage failed  {name}  {reason}",
			[UiTextKeys.Inventory.SecureContainerHeader] = "Secure Container",
			[UiTextKeys.Inventory.SecureInsuranceUninitialized] = "Insurance Uninitialized",
			[UiTextKeys.Inventory.SecureInsuranceRentalActive] = "Insurance Rental Active\nRuns Left {runs}\nPending Bill {bill}\nGreed {greed}%\nUnused Runs {unused_runs}\nChallenge Quota {quota}",
			[UiTextKeys.Inventory.SecureInsuranceChallengeActive] = "Insurance Challenge Active\nQuota Left {quota}\nPending Bill {bill}\nGreed {greed}%\nUnused Runs {unused_runs}",
			[UiTextKeys.Inventory.SecureInsuranceRetrievalOnly] = "Insurance Retrieval Only\nPending Bill {bill}\nGreed {greed}%",
			[UiTextKeys.Inventory.SecureInsuranceNone] = "Insurance None",
			[UiTextKeys.Inventory.SecureQuotaButton] = "Quota From Challenges",
			[UiTextKeys.Inventory.SecureRentalButton] = "Buy Rental Access +{runs} Runs -{cost}",
			[UiTextKeys.Inventory.SecureRentalExtendButton] = "Extend Rental +{runs} Runs -{cost}",
			[UiTextKeys.Inventory.SecureRentalType] = "Rental Access",
			[UiTextKeys.Inventory.InsufficientCredits] = "Insufficient Credits",
			[UiTextKeys.Inventory.DragHint] = "{name}  x{count}",
			[UiTextKeys.Inventory.HintToggleInventory] = "B Open/Close Inventory",
			[UiTextKeys.Inventory.HintTakeAll] = "F Take All",
			[UiTextKeys.Inventory.HintQuickTransfer] = "Ctrl+Left Click Quick Transfer",
			[UiTextKeys.Inventory.HintDragItem] = "Left click to drag items",
			[UiTextKeys.Inventory.HintRotateDragging] = "Right click while dragging to rotate",
			[UiTextKeys.Inventory.HintCloseContainer] = "Q Close Container",
			[UiTextKeys.Inventory.HintInspectSlot] = "Double click a slot to inspect",
			[UiTextKeys.Inventory.HintDragToEquip] = "Drag items onto slots to equip",
			[UiTextKeys.Inventory.HintUseItem] = "Right click to use an item",
			[UiTextKeys.Inventory.AccessMissingRequester] = "Access requires a valid requester",
			[UiTextKeys.Inventory.AccessLocked] = "Container is locked",
			[UiTextKeys.Inventory.AccessSingleUseConsumed] = "Container access has been consumed",
			[UiTextKeys.Inventory.AccessOutOfRange] = "Move closer ({current}/{required})",
			[UiTextKeys.Inventory.AccessMissingTag] = "Missing access tag {tag}",

			[UiTextKeys.World.ContainerUnavailable] = "Unavailable",
			[UiTextKeys.World.ContainerLocked] = "Locked",
			[UiTextKeys.World.ContainerConsumed] = "Used",
			[UiTextKeys.World.ContainerOpenable] = "Openable",
			[UiTextKeys.World.ContainerLabel] = "Chest\n{access}\nItems {item_count}",

			[UiTextKeys.Status.PlayerHeader] = "Player Status",
			[UiTextKeys.Status.Firearm] = "Firearm",
			[UiTextKeys.Status.Hit] = "Hit",
			[UiTextKeys.Status.Magazine] = "Magazine",
			[UiTextKeys.Status.Aim] = "Aim",
			[UiTextKeys.Status.On] = "On",
			[UiTextKeys.Status.Off] = "Off",
			[UiTextKeys.Status.FireMode] = "Fire Mode",
			[UiTextKeys.Status.FireModeAutomatic] = "Automatic",
			[UiTextKeys.Status.FireModeSelective] = "Selective",
			[UiTextKeys.Status.FireModeSingleShot] = "Single Shot",
			[UiTextKeys.Status.Health] = "Health",
			[UiTextKeys.Status.Stamina] = "Stamina",
			[UiTextKeys.Status.Attack] = "Attack",
			[UiTextKeys.Status.ActiveWeapon] = "Active Weapon",
			[UiTextKeys.Status.Defense] = "Defense",
			[UiTextKeys.Status.MoveSpeed] = "Move Speed",
			[UiTextKeys.Status.SearchMultiplier] = "Search Mult",
			[UiTextKeys.Status.HighRarity] = "High Rarity",
			[UiTextKeys.Status.ActiveSearchMods] = "Active Search Mods",
		}
	};

	public static event Action? LanguageChanged;

	public static string CurrentLocale { get; private set; } = DefaultLocale;

	public static void SetLocale(string locale)
	{
		string normalized = string.IsNullOrWhiteSpace(locale) ? DefaultLocale : locale;
		if (string.Equals(CurrentLocale, normalized, StringComparison.OrdinalIgnoreCase))
		{
			return;
		}

		CurrentLocale = normalized;
		LanguageChanged?.Invoke();
	}

	public static string Resolve(string key)
	{
		return Resolve(key, (IDictionary<string, object?>?)null);
	}

	public static string Resolve(string key, params (string Name, object? Value)[] args)
	{
		RuntimeObjectDictionary map = [];
		foreach ((string name, object? value) in args)
		{
			map[name] = value;
		}

		return Resolve(key, map);
	}

	public static string Resolve(string key, IDictionary<string, object?>? args)
	{
		string template = GetTemplate(CurrentLocale, key)
			?? GetTemplate(FallbackLocale, key)
			?? $"[{key}]";

		if (args == null || args.Count == 0)
		{
			return template;
		}

		string result = template;
		foreach ((string argKey, object? value) in args)
		{
			result = result.Replace("{" + argKey + "}", FormatValue(value), StringComparison.Ordinal);
		}

		return result;
	}

	public static string Resolve(string key, Godot.Collections.Dictionary<string, Variant>? args)
	{
		if (args == null || args.Count == 0)
		{
			return Resolve(key);
		}

		RuntimeObjectDictionary converted = [];
		foreach ((string argKey, Variant value) in args)
		{
			converted[argKey] = value.Obj;
		}

		return Resolve(key, converted);
	}

	private static string? GetTemplate(string locale, string key)
	{
		if (!Catalog.TryGetValue(locale, out RuntimeDictionary? table))
		{
			return null;
		}

		return table.TryGetValue(key, out string? value) ? value : null;
	}

	private static string FormatValue(object? value)
	{
		return value switch
		{
			null => string.Empty,
			float floatValue => floatValue.ToString("0.0", CultureInfo.InvariantCulture),
			double doubleValue => doubleValue.ToString("0.0", CultureInfo.InvariantCulture),
			IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
			_ => value.ToString() ?? string.Empty
		};
	}
}
