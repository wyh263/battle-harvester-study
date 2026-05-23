using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace BattleHarvesterStudy.Session;

// Run (运行、游戏会话) Session (会话)
// 整体翻译: 游戏运行会话管理器
// 功能: 该类是游戏的核心状态管理器，采用单例模式，负责管理整个游戏运行过程中的全局状态，
// 包括场景切换、玩家状态（背包、装备、仓库）的捕获与应用、游戏进度的跟踪（例如精英怪是否被击败、是否可撤离）。
// 它在不同的游戏场景（如基地、战斗地图）之间进行数据持久化和状态同步。
public partial class RunSession : Node
{
	// Instance (实例)
	// 功能: RunSession的静态单例实例，允许从游戏中的任何地方方便地访问其功能和状态。
	public static RunSession? Instance { get; private set; }
	private const string SaveGamePath = "user://savegame.json";
	private const int SaveGameVersion = 1;
	private const string CoreSaveSectionId = "run_session_core";
	public const string SaveGameParticipantGroupName = "save_game_participant";
	private bool _isLoadingSave;
	private bool _suppressAutosave;
	private Node3D? _boundPlayer;
	private InventoryComponent? _boundInventory;
	private EquipmentComponent? _boundEquipment;
	private readonly Dictionary<string, JsonElement> _loadedSaveSections = [];

	// Home (家) Scene (场景) Path (路径)
	// 功能: 定义玩家基地场景的文件路径。当玩家完成一次“Run”或失败时，会返回到此场景。
	[Export]
	public string HomeScenePath { get; set; } = "res://scenes/home_base.tscn";

	// Default (默认) Run (运行、会话) Scene (场景) Path (路径)
	// 功能: 定义默认的战斗场景文件路径。当玩家开始一次新的“Run”但没有指定具体场景时，会加载此场景。
	[Export]
	public string DefaultRunScenePath { get; set; } = "res://scenes/run_map_01.tscn";

	[Export]
	public string SettlementScenePath { get; set; } = "res://scenes/settlement_screen.tscn";

	[Export]
	public int SecureContainerChallengeQuotaReward { get; set; } = 100000;

	[Export]
	public float SecureContainerBaseGreedRate { get; set; } = 0.05f;

	[Export]
	public int SecureContainerRentalUsesPerPurchase { get; set; } = 3;

	[Export]
	public int SecureContainerGreedThresholdValue { get; set; } = 100000;

	[Export]
	public float SecureContainerGreedIncreasePerThreshold { get; set; } = 0.005f;

	[Export]
	public float SecureContainerGreedDecayPerUnusedRun { get; set; } = 0.01f;

	// Has (有) Active (活跃的) Run (运行、会话)
	// 功能: 指示当前是否存在一个正在进行中的游戏会话（即玩家是否在战斗场景中）。
	public bool HasActiveRun { get; private set; }
	// Elite (精英) Defeated (击败)
	// 功能: 指示当前游戏会话中，精英敌人（Boss）是否已被击败。
	public bool EliteDefeated { get; private set; }
	// Extraction (撤离) Unlocked (解锁)
	// 功能: 指示玩家是否已满足撤离条件，可以从当前场景撤离。
	public bool ExtractionUnlocked { get; private set; }
	public int PlayerCredits { get; private set; }
	public int LastExtractionLootValue { get; private set; }
	public SecureContainerInsuranceMode SecureContainerInsuranceMode { get; private set; }

	public const int SecureContainerRentalCost = 5000;
	public const string DefaultSecureContainerId = "player_secure_container";

	// _player (玩家) Inventory (背包) Snapshot (快照)
	// 功能: 存储玩家背包物品的快照，用于场景切换时持久化和恢复背包状态。
	private readonly List<ContainerItemSnapshot> _playerInventorySnapshot = [];
	// _equipment (装备) Snapshot (快照)
	// 功能: 存储玩家装备物品的快照，用于场景切换时持久化和恢复装备状态。
	private readonly Dictionary<EquipmentSlotType, ItemSnapshot> _equipmentSnapshot = [];
	// _warehouse (仓库) Snapshot (快照)
	// 功能: 存储玩家仓库物品的快照，用于场景切换时持久化和恢复仓库状态。
	private readonly List<ContainerItemSnapshot> _warehouseSnapshot = [];
	// _secure (安全) Container (容器) Snapshot (快照)
	// 功能: 存储安全容器（例如特殊保险箱）物品的快照，用于持久化和恢复。
	private readonly Dictionary<string, List<ContainerItemSnapshot>> _secureContainerSnapshots = [];
	private readonly List<SkillDefinition> _learnedWeaponSkillsSnapshot = [];
	private readonly Dictionary<string, SecureContainerRuntimeState> _secureContainerStates = [];
	private SecureContainerRuntimeState DefaultSecureContainerState => GetOrCreateSecureContainerState(DefaultSecureContainerId);
	// _player (玩家) State (状态) Initialized (已初始化)
	// 功能: 标记玩家状态（背包、装备）是否已经初始化过（即是否已从玩家节点捕获过一次快照）。
	private bool _playerStateInitialized;

	// _Enter (进入) Tree (树)
	// 功能: Godot生命周期方法，当节点进入场景树时调用。在此处设置RunSession的单例实例。
	public override void _EnterTree()
	{
		Instance = this;
		InitializeSecureContainerState();
		LoadGameFromDisk();
	}

	// _Exit (退出) Tree (树)
	// 功能: Godot生命周期方法，当节点从场景树中退出时调用。在此处清除RunSession的单例实例，防止出现野指针或重复引用。
	public override void _ExitTree()
	{
		UnbindAutosaveSources();
		if (Instance == this)
		{
			Instance = null;
		}
	}

	// _Unhandled (未处理的) Input (输入)
	// 功能: Godot生命周期方法，用于处理游戏中未被其他控件或节点处理的输入事件。这里用于处理"ui_cancel"操作，例如退出全屏模式。
	public override void _UnhandledInput(InputEvent @event)
	{
		_ = @event;
		// Is (是) Action (动作) Pressed (按下)
		// 整体翻译: 动作是否被按下
		// 功能: 检查给定的输入事件是否是指定动作（"ui_cancel"）的按下事件。
		if (@event.IsActionPressed("ui_cancel") && OS.HasFeature("__legacy_fullscreen_exit__"))
		{
			// Get (获取) Viewport (视口)
			// 整体翻译: 获取视口
			// 功能: 获取当前节点的 Viewport 对象。
			// Set (设置) Input (输入) As (作为) Handled (已处理)
			// 整体翻译: 将输入标记为已处理
			// 功能: 标记当前输入事件已处理，防止事件继续传播到其他节点。
			GetViewport().SetInputAsHandled();

			// Display (显示) Server (服务器) Window (窗口) Get (获取) Mode (模式)
			// 整体翻译: 获取显示服务器窗口模式
			// 功能: 获取当前游戏窗口的显示模式（例如，独占全屏、全屏、窗口化）。
			// Display (显示) Server (服务器) Window (窗口) Set (设置) Mode (模式)
			// 整体翻译: 设置显示服务器窗口模式
			// 功能: 设置当前游戏窗口的显示模式。
			if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.ExclusiveFullscreen
				|| DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen)
			{
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
			}
		}
	}

	// Bind (绑定) Player (玩家)
	// 功能: 绑定玩家节点，并在必要时捕获玩家的初始状态（背包和装备）或应用之前存储的状态。
	public void BindPlayer(Node3D? player)
	{
		if (player == null)
		{
			return;
		}

		BindAutosaveSources(player);
		if (!_playerStateInitialized)
		{
			CapturePlayerState(player);
			_playerStateInitialized = true;
			SaveGameToDisk();
		}

		_suppressAutosave = true;
		try
		{
			ApplyPlayerState(player);
		}
		finally
		{
			_suppressAutosave = false;
		}
	}

	// Persist (持久化) Player (玩家) State (状态)
	// 功能: 捕获玩家当前状态（背包和装备）并将其存储为快照，以便在场景切换后可以恢复。通常在玩家离开当前场景前调用。
	public void PersistPlayerState(Node3D? player)
	{
		if (player == null)
		{
			return;
		}

		BindAutosaveSources(player);
		CapturePlayerState(player);
		_playerStateInitialized = true;
		SaveGameToDisk();
	}

	// Start (开始) Run (运行、会话)
	// 功能: 启动一次新的游戏会话（即进入战斗场景）。它会首先持久化玩家当前状态，重置会话标志，然后切换到指定的战斗场景。
	public void StartRun(Node3D? player, string? scenePath = null)
	{
		if (!HasActiveRun && IsInHomeScene(player))
		{
			NormalizeOwnedItemsAsBase(player);
		}

		BeginSecureContainerRun();
		PersistPlayerState(player);
		HasActiveRun = true;
		EliteDefeated = false;
		SetExtractionUnlocked(false);
		SaveGameToDisk();
		// Get (获取) Tree (树)
		// 整体翻译: 获取场景树
		// 功能: 获取当前节点所属的 SceneTree 对象。
		// Change (改变) Scene (场景) To (到) File (文件)
		// 整体翻译: 改变场景到文件
		// 功能: 将当前场景切换为指定文件路径的场景。
		// string (字符串) Is (是) Null (空) Or (或者) White (空白) Space (空间)
		// 整体翻译: 字符串是否为空或空白
		// 功能: 检查一个字符串是否为 null、空字符串或只包含空白字符。
		GetTree().ChangeSceneToFile(string.IsNullOrWhiteSpace(scenePath) ? DefaultRunScenePath : scenePath);
	}

	// Complete (完成) Extraction (撤离)
	// 功能: 完成一次成功的撤离。它会持久化玩家当前状态，重置会话标志，然后返回基地场景。
	public void CompleteExtraction(Node3D? player)
	{
		FinalizeSecureContainerRun();
		LastExtractionLootValue = GetCurrentRunLootValue(player);
		NormalizeOwnedItemsAsBase(player);
		PersistPlayerState(player);
		HasActiveRun = false;
		EliteDefeated = false;
		SetExtractionUnlocked(false);
		SaveGameToDisk();
		// Get (获取) Tree (树)
		// 整体翻译: 获取场景树
		// 功能: 获取当前节点所属的 SceneTree 对象。
		// Change (改变) Scene (场景) To (到) File (文件)
		// 整体翻译: 改变场景到文件
		// 功能: 将当前场景切换为指定文件路径的场景。
		GetTree().ChangeSceneToFile(SettlementScenePath);
	}

	// Fail (失败) Run (运行、会话) And (和) Return (返回) Home (家)
	// 功能: 当游戏会话失败时调用。它会清除玩家运行时状态，重置会话标志，然后强制返回基地场景。
	public void FailRunAndReturnHome()
	{
		FinalizeSecureContainerRun();
		LastExtractionLootValue = 0;
		ClearPlayerRuntimeState();
		HasActiveRun = false;
		EliteDefeated = false;
		SetExtractionUnlocked(false);
		SaveGameToDisk();
		// Get (获取) Tree (树)
		// 整体翻译: 获取场景树
		// 功能: 获取当前节点所属的 SceneTree 对象。
		// Change (改变) Scene (场景) To (到) File (文件)
		// 整体翻译: 改变场景到文件
		// 功能: 将当前场景切换为指定文件路径的场景。
		GetTree().ChangeSceneToFile(HomeScenePath);
	}

	// Mark (标记) Elite (精英) Defeated (击败)
	// 功能: 标记当前游戏会话中的精英敌人已被击败，并解锁撤离功能。
	public void MarkEliteDefeated()
	{
		if (EliteDefeated)
		{
			return;
		}

		EliteDefeated = true;
		GrantSecureContainerQuotaFromChallenge(SecureContainerChallengeQuotaReward);
		SetExtractionUnlocked(true);
		SaveGameToDisk();
	}

	// Apply (应用) Warehouse (仓库) State (状态)
	// 功能: 将之前捕获的仓库物品快照应用到指定的仓库容器组件。
	public void ApplyWarehouseState(GridContainerComponent? container)
	{
		ApplyContainerSnapshot(container, _warehouseSnapshot);
	}

	// Persist (持久化) Warehouse (仓库) State (状态)
	// 功能: 捕获指定仓库容器组件的当前物品状态，并存储为仓库快照。
	public void PersistWarehouseState(GridContainerComponent? container)
	{
		CaptureContainerSnapshot(container, _warehouseSnapshot);
		SaveGameToDisk();
	}

	// Apply (应用) Secure (安全) Container (容器) State (状态)
	// 功能: 将之前捕获的安全容器物品快照应用到指定容器组件。
	public void ApplySecureContainerState(GridContainerComponent? container)
	{
		string containerId = ResolveSecureContainerId(container);
		ApplyContainerSnapshot(container, GetOrCreateSecureContainerSnapshot(containerId));
	}

	// Persist (持久化) Secure (安全) Container (容器) State (状态)
	// 功能: 捕获指定安全容器组件的当前物品状态，并存储为安全容器快照。
	public void PersistSecureContainerState(GridContainerComponent? container)
	{
		string containerId = ResolveSecureContainerId(container);
		CaptureContainerSnapshot(container, GetOrCreateSecureContainerSnapshot(containerId));
		SaveGameToDisk();
	}

	// Set (设置) Extraction (撤离) Unlocked (解锁)
	// 功能: 根据传入的布尔值设置ExtractionUnlocked状态，并避免不必要的重复设置。
	private void SetExtractionUnlocked(bool unlocked)
	{
		if (ExtractionUnlocked == unlocked)
		{
			return;
		}

		ExtractionUnlocked = unlocked;
	}

	// Capture (捕获) Player (玩家) State (状态)
	// 功能: 从给定的玩家节点中捕获其背包和装备的当前状态，并存储为快照。
	private void CapturePlayerState(Node3D player)
	{
		CaptureContainerSnapshot(GetPlayerInventoryContainer(player), _playerInventorySnapshot);
		CaptureEquipmentSnapshot(GetPlayerEquipment(player), _equipmentSnapshot);
		CaptureLearnedWeaponSkills(GetPlayerWeaponSkillKnowledge(player), _learnedWeaponSkillsSnapshot);
	}

	// Apply (应用) Player (玩家) State (状态)
	// 功能: 将之前存储的玩家背包和装备快照应用到给定的玩家节点。
	private void ApplyPlayerState(Node3D player)
	{
		ApplyContainerSnapshot(GetPlayerInventoryContainer(player), _playerInventorySnapshot);
		ApplyEquipmentSnapshot(GetPlayerEquipment(player), _equipmentSnapshot);
		ApplyLearnedWeaponSkills(GetPlayerWeaponSkillKnowledge(player), _learnedWeaponSkillsSnapshot);
	}

	// Clear (清除) Player (玩家) Runtime (运行时) State (状态)
	// 功能: 清除玩家的背包和装备快照，通常在游戏会话失败时调用，以防止状态残留。
	private void ClearPlayerRuntimeState()
	{
		_playerInventorySnapshot.Clear();
		_equipmentSnapshot.Clear();
		_learnedWeaponSkillsSnapshot.Clear();
	}

	private void BindAutosaveSources(Node3D player)
	{
		if (_boundPlayer == player)
		{
			return;
		}

		UnbindAutosaveSources();
		_boundPlayer = player;
		_boundInventory = player.GetNodeOrNull<InventoryComponent>("Components/Inventory");
		_boundEquipment = player.GetNodeOrNull<EquipmentComponent>("Components/Equipment");

		if (_boundInventory != null)
		{
			_boundInventory.InventoryChanged += OnBoundPlayerStateChanged;
		}

		if (_boundEquipment != null)
		{
			_boundEquipment.EquipmentChanged += OnBoundPlayerStateChanged;
		}
	}

	private void UnbindAutosaveSources()
	{
		if (_boundInventory != null)
		{
			_boundInventory.InventoryChanged -= OnBoundPlayerStateChanged;
			_boundInventory = null;
		}

		if (_boundEquipment != null)
		{
			_boundEquipment.EquipmentChanged -= OnBoundPlayerStateChanged;
			_boundEquipment = null;
		}
	}

	private void OnBoundPlayerStateChanged()
	{
		if (_suppressAutosave)
		{
			return;
		}

		if (_boundPlayer != null && IsInstanceValid(_boundPlayer))
		{
			PersistPlayerState(_boundPlayer);
		}
	}

	// Get (获取) Player (玩家) Inventory (背包) Container (容器)
	// 功能: 从给定的玩家节点中获取其主背包容器组件。
	private static GridContainerComponent? GetPlayerInventoryContainer(Node3D player)
	{
		InventoryComponent? inventory = player.GetNodeOrNull<InventoryComponent>("Components/Inventory");
		return inventory?.GetPrimaryContainer();
	}

	// Get (获取) Player (玩家) Equipment (装备)
	// 功能: 从给定的玩家节点中获取其装备组件。
	private static EquipmentComponent? GetPlayerEquipment(Node3D player)
	{
		return player.GetNodeOrNull<EquipmentComponent>("Components/Equipment");
	}

	private static WeaponSkillKnowledgeComponent? GetPlayerWeaponSkillKnowledge(Node3D player)
	{
		return player.GetNodeOrNull<WeaponSkillKnowledgeComponent>("Components/WeaponSkillKnowledge");
	}

	private static GridContainerComponent? GetPlayerSecureContainer(Node3D player)
	{
		return player.GetNodeOrNull<GridContainerComponent>("Components/SecureContainer");
	}

	private bool IsInHomeScene(Node3D player)
	{
		return string.Equals(
			player.GetTree().CurrentScene?.SceneFilePath,
			HomeScenePath,
			System.StringComparison.OrdinalIgnoreCase);
	}

	// Capture (捕获) Container (容器) Snapshot (快照)
	// 功能: 捕获指定容器组件的当前物品状态，并将其存储到一个物品快照列表中。
	private static void CaptureContainerSnapshot(GridContainerComponent? container, List<ContainerItemSnapshot> snapshot)
	{
		snapshot.Clear();
		if (container == null)
		{
			return;
		}

		foreach (ContainerItemRecord record in container.ItemRecords)
		{
			snapshot.Add(new ContainerItemSnapshot
			{
				Item = ItemSnapshot.FromItem(record.Item),
				Origin = record.Origin
			});
		}
	}

	// Apply (应用) Container (容器) Snapshot (快照)
	// 功能: 将物品快照列表中的物品应用到指定的容器组件，清空原容器内容并重新填充。
	private static void ApplyContainerSnapshot(GridContainerComponent? container, List<ContainerItemSnapshot> snapshot)
	{
		if (container == null)
		{
			return;
		}

		container.Clear();
		foreach (ContainerItemSnapshot itemSnapshot in snapshot)
		{
			container.TryPlaceIncomingItemAt(itemSnapshot.Item.ToItem(), itemSnapshot.Origin, itemSnapshot.Item.IsRotated);
		}
	}

	// Capture (捕获) Equipment (装备) Snapshot (快照)
	// 功能: 捕获指定装备组件的当前装备状态，并将其存储到一个装备快照字典中。
	private static void CaptureEquipmentSnapshot(
		EquipmentComponent? equipment,
		Dictionary<EquipmentSlotType, ItemSnapshot> snapshot)
	{
		snapshot.Clear();
		if (equipment == null)
		{
			return;
		}

		foreach ((EquipmentSlotType slotType, EquipmentSlotRecord slot) in equipment.Slots)
		{
			if (slot.EquippedItem == null)
			{
				continue;
			}

			snapshot[slotType] = ItemSnapshot.FromItem(slot.EquippedItem);
		}
	}

	// Apply (应用) Equipment (装备) Snapshot (快照)
	// 功能: 将装备快照字典中的装备应用到指定的装备组件，清空原装备槽并重新填充。
	private static void ApplyEquipmentSnapshot(
		EquipmentComponent? equipment,
		Dictionary<EquipmentSlotType, ItemSnapshot> snapshot)
	{
		if (equipment == null)
		{
			return;
		}

		equipment.ClearAllSlots();
		foreach ((EquipmentSlotType slotType, ItemSnapshot itemSnapshot) in snapshot)
		{
			equipment.TryEquip(slotType, itemSnapshot.ToItem(), out _, out _);
		}
	}

	private static void CaptureLearnedWeaponSkills(
		WeaponSkillKnowledgeComponent? knowledge,
		List<SkillDefinition> snapshot)
	{
		snapshot.Clear();
		if (knowledge == null)
		{
			return;
		}

		foreach (SkillDefinition skill in knowledge.LearnedSkills)
		{
			snapshot.Add(skill);
		}
	}

	private static void ApplyLearnedWeaponSkills(
		WeaponSkillKnowledgeComponent? knowledge,
		List<SkillDefinition> snapshot)
	{
		knowledge?.RestoreLearnedSkills(snapshot);
	}

	// Container (容器) Item (物品) Snapshot (快照)
	// 整体翻译: 容器物品快照
	// 功能: 这是一个内部类，用于表示容器（如背包、仓库）中单个物品的快照数据，包含物品本身和它在容器中的原始位置。
	private sealed class ContainerItemSnapshot
	{
		// Item (物品)
		// 功能: 物品本身的快照数据。
		public required ItemSnapshot Item { get; init; }
		// Origin (原点、起始位置)
		// 功能: 物品在容器网格中的起始坐标（左上角）。
		public required Vector2I Origin { get; init; }
	}

	// Item (物品) Snapshot (快照)
	// 整体翻译: 物品快照
	// 功能: 这是一个内部类，用于表示单个物品实例的快照数据，包含物品的定义、堆叠数量、是否旋转以及剩余使用次数等，以便在场景切换时进行持久化。
	private sealed class ItemSnapshot
	{
		// Definition (定义)
		// 功能: 物品的定义资源，包含物品的基本属性。
		public required ItemDefinition Definition { get; init; }
		// Stack (堆叠) Count (数量)
		// 功能: 物品堆叠的数量。
		public required int StackCount { get; init; }
		// Is (是否) Rotated (旋转)
		// 功能: 物品在容器中是否旋转。
		public required bool IsRotated { get; init; }
		// Remaining (剩余的) Uses (使用次数)
		// 功能: 物品剩余的使用次数（如果物品有使用次数限制）。
	public required int RemainingUses { get; init; }
	public required float CurrentDurability { get; init; }
	public required float CurrentMaxDurability { get; init; }
	public required float CurrentArmorPoint { get; init; }
	public required float CurrentMaxArmorPoint { get; init; }
	public required int CurrentMagazineAmmo { get; init; }
	public required string LoadedAmmoItemId { get; init; }
	public required AmmoType LoadedAmmoType { get; init; }
	public required int LoadedAmmoTier { get; init; }
	public required float LoadedAmmoPenetrationTier { get; init; }
	public required ItemAcquisitionState AcquisitionState { get; init; }
	public required int RunLootStackCount { get; init; }
	public required List<InstalledWeaponSkillState> WeaponSkillSlots { get; init; }

		// From (从...创建) Item (物品)
		// 功能: 一个静态工厂方法，从一个ItemInstance对象创建并返回一个ItemSnapshot实例。
		public static ItemSnapshot FromItem(ItemInstance item)
		{
			return new ItemSnapshot
			{
				Definition = item.Definition,
				StackCount = item.StackCount,
				IsRotated = item.IsRotated,
			RemainingUses = item.RemainingUses,
			CurrentDurability = item.CurrentDurability,
			CurrentMaxDurability = item.CurrentMaxDurability,
			CurrentArmorPoint = item.CurrentArmorPoint,
			CurrentMaxArmorPoint = item.CurrentMaxArmorPoint,
			CurrentMagazineAmmo = item.CurrentMagazineAmmo,
			LoadedAmmoItemId = item.LoadedAmmoItemId,
			LoadedAmmoType = item.LoadedAmmoType,
			LoadedAmmoTier = item.LoadedAmmoTier,
			LoadedAmmoPenetrationTier = item.LoadedAmmoPenetrationTier,
			AcquisitionState = item.AcquisitionState,
			RunLootStackCount = item.RunLootStackCount,
			WeaponSkillSlots = new List<InstalledWeaponSkillState>(item.WeaponSkillSlots.Select(slot => slot.CreateCopy())),
		};
	}

		// To (转换为) Item (物品)
		// 功能: 将当前的ItemSnapshot实例转换回一个ItemInstance对象。
		public ItemInstance ToItem()
		{
		ItemInstance item = new(Definition, StackCount, IsRotated, remainingUses: RemainingUses);
		item.RestoreRuntimeState(CurrentDurability, CurrentMaxDurability, CurrentArmorPoint, CurrentMaxArmorPoint);
		item.RestoreMagazineAmmo(CurrentMagazineAmmo);
		item.RestoreLoadedAmmo(LoadedAmmoItemId, LoadedAmmoType, LoadedAmmoTier, LoadedAmmoPenetrationTier);
		item.RestoreAcquisitionState(AcquisitionState, RunLootStackCount);
		item.RestoreWeaponSkillSlots(WeaponSkillSlots);
		return item;
	}
	}

	public void SaveGameToDisk()
	{
		if (_isLoadingSave || _suppressAutosave)
		{
			return;
		}

		if (_boundPlayer != null && IsInstanceValid(_boundPlayer))
		{
			CapturePlayerState(_boundPlayer);
			_playerStateInitialized = true;
		}

		SaveGameFile data = CreateSaveGameFile();
		JsonSerializerOptions options = new() { WriteIndented = true };
		string json = JsonSerializer.Serialize(data, options);
		using FileAccess? file = FileAccess.Open(SaveGamePath, FileAccess.ModeFlags.Write);
		if (file == null)
		{
			GD.PushWarning($"Failed to open save file for writing: {SaveGamePath}");
			return;
		}

		file.StoreString(json);
	}

	public bool LoadGameFromDisk()
	{
		if (!FileAccess.FileExists(SaveGamePath))
		{
			return false;
		}

		using FileAccess? file = FileAccess.Open(SaveGamePath, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.PushWarning($"Failed to open save file for reading: {SaveGamePath}");
			return false;
		}

		try
		{
			string json = file.GetAsText();
			SaveGameFile? saveFile = JsonSerializer.Deserialize<SaveGameFile>(json);
			SaveGameData? data = null;
			if (saveFile?.Sections.TryGetValue(CoreSaveSectionId, out JsonElement coreSection) == true)
			{
				data = coreSection.Deserialize<SaveGameData>();
				_loadedSaveSections.Clear();
				foreach ((string sectionId, JsonElement sectionData) in saveFile.Sections)
				{
					if (sectionId != CoreSaveSectionId)
					{
						_loadedSaveSections[sectionId] = sectionData.Clone();
					}
				}
			}
			else
			{
				// Legacy flat save format from the first save implementation.
				data = JsonSerializer.Deserialize<SaveGameData>(json);
				_loadedSaveSections.Clear();
			}

			if (data == null)
			{
				return false;
			}

			_isLoadingSave = true;
			ApplySaveGameData(data);
			RestoreRegisteredSaveParticipants();
			return true;
		}
		catch (Exception exception)
		{
			GD.PushWarning($"Failed to load save file {SaveGamePath}: {exception.Message}");
			return false;
		}
		finally
		{
			_isLoadingSave = false;
		}
	}

	private SaveGameFile CreateSaveGameFile()
	{
		Dictionary<string, JsonElement> sections = new(_loadedSaveSections);
		sections[CoreSaveSectionId] = JsonSerializer.SerializeToElement(CreateSaveGameData());
		foreach ((string sectionId, JsonElement sectionData) in CaptureRegisteredSaveSections())
		{
			sections[sectionId] = sectionData;
		}

		return new SaveGameFile
		{
			Version = SaveGameVersion,
			Sections = sections
		};
	}

	private SaveGameData CreateSaveGameData()
	{
		return new SaveGameData
		{
			Version = SaveGameVersion,
			HasActiveRun = HasActiveRun,
			EliteDefeated = EliteDefeated,
			ExtractionUnlocked = ExtractionUnlocked,
			PlayerCredits = PlayerCredits,
			LastExtractionLootValue = LastExtractionLootValue,
			PlayerStateInitialized = _playerStateInitialized,
			PlayerInventory = ToContainerSaveList(_playerInventorySnapshot),
			Equipment = _equipmentSnapshot
				.Select(pair => new EquipmentItemSaveData
				{
					SlotType = (int)pair.Key,
					Item = ToItemSaveData(pair.Value)
				})
				.ToList(),
			Warehouse = ToContainerSaveList(_warehouseSnapshot),
			SecureContainers = _secureContainerSnapshots.ToDictionary(
				pair => pair.Key,
				pair => ToContainerSaveList(pair.Value)),
			LearnedWeaponSkillPaths = _learnedWeaponSkillsSnapshot
				.Select(GetResourcePath)
				.Where(path => !string.IsNullOrWhiteSpace(path))
				.ToList(),
			SecureContainerStates = _secureContainerStates.ToDictionary(
				pair => pair.Key,
				pair => ToSecureStateSaveData(pair.Value))
		};
	}

	private void ApplySaveGameData(SaveGameData data)
	{
		HasActiveRun = data.HasActiveRun;
		EliteDefeated = data.EliteDefeated;
		ExtractionUnlocked = data.ExtractionUnlocked;
		PlayerCredits = Mathf.Max(0, data.PlayerCredits);
		LastExtractionLootValue = Mathf.Max(0, data.LastExtractionLootValue);
		_playerStateInitialized = data.PlayerStateInitialized;

		ApplyContainerSaveList(data.PlayerInventory, _playerInventorySnapshot);
		_equipmentSnapshot.Clear();
		foreach (EquipmentItemSaveData equipmentItem in data.Equipment)
		{
			ItemSnapshot? item = FromItemSaveData(equipmentItem.Item);
			if (item != null)
			{
				_equipmentSnapshot[(EquipmentSlotType)equipmentItem.SlotType] = item;
			}
		}

		ApplyContainerSaveList(data.Warehouse, _warehouseSnapshot);
		_secureContainerSnapshots.Clear();
		foreach ((string containerId, List<ContainerItemSaveData> items) in data.SecureContainers)
		{
			List<ContainerItemSnapshot> snapshot = [];
			ApplyContainerSaveList(items, snapshot);
			_secureContainerSnapshots[containerId] = snapshot;
		}

		_learnedWeaponSkillsSnapshot.Clear();
		foreach (string skillPath in data.LearnedWeaponSkillPaths)
		{
			SkillDefinition? skill = LoadResourceOrNull<SkillDefinition>(skillPath);
			if (skill != null)
			{
				_learnedWeaponSkillsSnapshot.Add(skill);
			}
		}

		_secureContainerStates.Clear();
		foreach ((string containerId, SecureContainerStateSaveData stateData) in data.SecureContainerStates)
		{
			_secureContainerStates[containerId] = FromSecureStateSaveData(stateData, containerId);
		}

		if (_secureContainerStates.Count == 0)
		{
			InitializeSecureContainerState();
		}
		else
		{
			RefreshSecureContainerInsuranceMode();
		}
	}

	public bool TryGetSaveSection<T>(string sectionId, out T? sectionData)
	{
		sectionData = default;
		if (string.IsNullOrWhiteSpace(sectionId)
			|| !_loadedSaveSections.TryGetValue(sectionId, out JsonElement element))
		{
			return false;
		}

		try
		{
			sectionData = element.Deserialize<T>();
			return sectionData != null;
		}
		catch (Exception exception)
		{
			GD.PushWarning($"Failed to deserialize save section {sectionId}: {exception.Message}");
			return false;
		}
	}

	public void RestoreRegisteredSaveParticipants()
	{
		foreach (ISaveGameParticipant participant in GetRegisteredSaveParticipants())
		{
			if (_loadedSaveSections.TryGetValue(participant.SaveSectionId, out JsonElement sectionData))
			{
				participant.RestoreSaveSection(sectionData);
			}
		}
	}

	private Dictionary<string, JsonElement> CaptureRegisteredSaveSections()
	{
		Dictionary<string, JsonElement> sections = [];
		foreach (ISaveGameParticipant participant in GetRegisteredSaveParticipants())
		{
			string sectionId = participant.SaveSectionId;
			if (string.IsNullOrWhiteSpace(sectionId) || sectionId == CoreSaveSectionId)
			{
				continue;
			}

			sections[sectionId] = participant.CaptureSaveSection().Clone();
		}

		return sections;
	}

	private IEnumerable<ISaveGameParticipant> GetRegisteredSaveParticipants()
	{
		SceneTree? tree = GetTree();
		if (tree == null)
		{
			yield break;
		}

		foreach (Node node in tree.GetNodesInGroup(SaveGameParticipantGroupName))
		{
			if (node is ISaveGameParticipant participant)
			{
				yield return participant;
			}
		}
	}

	private static List<ContainerItemSaveData> ToContainerSaveList(List<ContainerItemSnapshot> snapshot)
	{
		return snapshot
			.Select(item => new ContainerItemSaveData
			{
				OriginX = item.Origin.X,
				OriginY = item.Origin.Y,
				Item = ToItemSaveData(item.Item)
			})
			.ToList();
	}

	private static void ApplyContainerSaveList(List<ContainerItemSaveData> saveItems, List<ContainerItemSnapshot> target)
	{
		target.Clear();
		foreach (ContainerItemSaveData saveItem in saveItems)
		{
			ItemSnapshot? item = FromItemSaveData(saveItem.Item);
			if (item == null)
			{
				continue;
			}

			target.Add(new ContainerItemSnapshot
			{
				Item = item,
				Origin = new Vector2I(saveItem.OriginX, saveItem.OriginY)
			});
		}
	}

	private static ItemSaveData ToItemSaveData(ItemSnapshot item)
	{
		return new ItemSaveData
		{
			DefinitionPath = GetResourcePath(item.Definition),
			StackCount = item.StackCount,
			IsRotated = item.IsRotated,
			RemainingUses = item.RemainingUses,
			CurrentDurability = item.CurrentDurability,
			CurrentMaxDurability = item.CurrentMaxDurability,
			CurrentArmorPoint = item.CurrentArmorPoint,
			CurrentMaxArmorPoint = item.CurrentMaxArmorPoint,
			CurrentMagazineAmmo = item.CurrentMagazineAmmo,
			LoadedAmmoItemId = item.LoadedAmmoItemId,
			LoadedAmmoType = (int)item.LoadedAmmoType,
			LoadedAmmoTier = item.LoadedAmmoTier,
			LoadedAmmoPenetrationTier = item.LoadedAmmoPenetrationTier,
			AcquisitionState = (int)item.AcquisitionState,
			RunLootStackCount = item.RunLootStackCount,
			WeaponSkillSlots = item.WeaponSkillSlots.Select(ToWeaponSkillSlotSaveData).ToList()
		};
	}

	private static ItemSnapshot? FromItemSaveData(ItemSaveData data)
	{
		ItemDefinition? definition = LoadResourceOrNull<ItemDefinition>(data.DefinitionPath);
		if (definition == null)
		{
			return null;
		}

		return new ItemSnapshot
		{
			Definition = definition,
			StackCount = Mathf.Max(1, data.StackCount),
			IsRotated = data.IsRotated,
			RemainingUses = Mathf.Max(0, data.RemainingUses),
			CurrentDurability = Mathf.Max(0.0f, data.CurrentDurability),
			CurrentMaxDurability = Mathf.Max(0.0f, data.CurrentMaxDurability),
			CurrentArmorPoint = Mathf.Max(0.0f, data.CurrentArmorPoint),
			CurrentMaxArmorPoint = Mathf.Max(0.0f, data.CurrentMaxArmorPoint),
			CurrentMagazineAmmo = Mathf.Max(0, data.CurrentMagazineAmmo),
			LoadedAmmoItemId = data.LoadedAmmoItemId ?? string.Empty,
			LoadedAmmoType = (AmmoType)data.LoadedAmmoType,
			LoadedAmmoTier = Mathf.Max(0, data.LoadedAmmoTier),
			LoadedAmmoPenetrationTier = Mathf.Max(0.0f, data.LoadedAmmoPenetrationTier),
			AcquisitionState = (ItemAcquisitionState)data.AcquisitionState,
			RunLootStackCount = Mathf.Max(0, data.RunLootStackCount),
			WeaponSkillSlots = data.WeaponSkillSlots.Select(FromWeaponSkillSlotSaveData).ToList()
		};
	}

	private static WeaponSkillSlotSaveData ToWeaponSkillSlotSaveData(InstalledWeaponSkillState slot)
	{
		return new WeaponSkillSlotSaveData
		{
			SlotIndex = slot.SlotIndex,
			SkillPath = GetResourcePath(slot.Skill),
			SourceItemDefinitionPath = GetResourcePath(slot.SourceItemDefinition),
			SourceAcquisitionState = (int)slot.SourceAcquisitionState,
			RemainingUses = slot.RemainingUses,
			ConsumeUsesOnCast = slot.ConsumeUsesOnCast,
			PermanentlyUnlocked = slot.PermanentlyUnlocked,
			CanBeLearned = slot.CanBeLearned,
			UnlockConditionType = (int)slot.UnlockConditionType,
			RequiredUseCount = slot.RequiredUseCount,
			RequiredKillCount = slot.RequiredKillCount,
			RequiredBossKillCount = slot.RequiredBossKillCount,
			CurrentUseCount = slot.CurrentUseCount,
			CurrentKillCount = slot.CurrentKillCount,
			CurrentBossKillCount = slot.CurrentBossKillCount
		};
	}

	private static InstalledWeaponSkillState FromWeaponSkillSlotSaveData(WeaponSkillSlotSaveData data)
	{
		return InstalledWeaponSkillState.Restore(
			data.SlotIndex,
			LoadResourceOrNull<SkillDefinition>(data.SkillPath),
			LoadResourceOrNull<ItemDefinition>(data.SourceItemDefinitionPath),
			(ItemAcquisitionState)data.SourceAcquisitionState,
			data.RemainingUses,
			data.ConsumeUsesOnCast,
			data.PermanentlyUnlocked,
			data.CanBeLearned,
			(WeaponSkillUnlockConditionType)data.UnlockConditionType,
			data.RequiredUseCount,
			data.RequiredKillCount,
			data.RequiredBossKillCount,
			data.CurrentUseCount,
			data.CurrentKillCount,
			data.CurrentBossKillCount);
	}

	private static SecureContainerStateSaveData ToSecureStateSaveData(SecureContainerRuntimeState state)
	{
		return new SecureContainerStateSaveData
		{
			ContainerId = state.ContainerId,
			RentalUsesRemaining = state.RentalUsesRemaining,
			ChallengeQuotaRemaining = state.ChallengeQuotaRemaining,
			PendingBill = state.PendingBill,
			CumulativeFeeCharged = state.CumulativeFeeCharged,
			CurrentRunRecordedValue = state.CurrentRunRecordedValue,
			UnusedRunCount = state.UnusedRunCount,
			BaseGreedRate = state.BaseGreedRate,
			CurrentGreedRate = state.CurrentGreedRate,
			UsedThisRun = state.UsedThisRun,
			RecordedItemIdsThisRun = state.RecordedItemIdsThisRun.ToList(),
			CurrentRunBillEntries = state.CurrentRunBillEntries.Select(ToBillEntrySaveData).ToList(),
			PendingBillEntries = state.PendingBillEntries.Select(ToBillEntrySaveData).ToList()
		};
	}

	private static SecureContainerRuntimeState FromSecureStateSaveData(SecureContainerStateSaveData data, string fallbackContainerId)
	{
		SecureContainerRuntimeState state = new()
		{
			ContainerId = string.IsNullOrWhiteSpace(data.ContainerId) ? fallbackContainerId : data.ContainerId,
			RentalUsesRemaining = Mathf.Max(0, data.RentalUsesRemaining),
			ChallengeQuotaRemaining = Mathf.Max(0, data.ChallengeQuotaRemaining),
			PendingBill = Mathf.Max(0, data.PendingBill),
			CumulativeFeeCharged = Mathf.Max(0, data.CumulativeFeeCharged),
			CurrentRunRecordedValue = Mathf.Max(0, data.CurrentRunRecordedValue),
			UnusedRunCount = Mathf.Max(0, data.UnusedRunCount),
			BaseGreedRate = Mathf.Max(0.0f, data.BaseGreedRate),
			CurrentGreedRate = Mathf.Max(0.0f, data.CurrentGreedRate),
			UsedThisRun = data.UsedThisRun
		};

		foreach (string instanceId in data.RecordedItemIdsThisRun)
		{
			if (!string.IsNullOrWhiteSpace(instanceId))
			{
				state.RecordedItemIdsThisRun.Add(instanceId);
			}
		}

		state.CurrentRunBillEntries.AddRange(data.CurrentRunBillEntries.Select(FromBillEntrySaveData));
		state.PendingBillEntries.AddRange(data.PendingBillEntries.Select(FromBillEntrySaveData));
		return state;
	}

	private static BillEntrySaveData ToBillEntrySaveData(SecureContainerBillEntry entry)
	{
		return new BillEntrySaveData
		{
			SourceInstanceId = entry.SourceInstanceId,
			DisplayName = entry.DisplayName,
			AssessedValue = entry.AssessedValue,
			FeeCharged = entry.FeeCharged
		};
	}

	private static SecureContainerBillEntry FromBillEntrySaveData(BillEntrySaveData data)
	{
		return new SecureContainerBillEntry(
			data.SourceInstanceId ?? string.Empty,
			data.DisplayName ?? string.Empty,
			Mathf.Max(0, data.AssessedValue),
			Mathf.Max(0, data.FeeCharged));
	}

	private static string GetResourcePath(Resource? resource)
	{
		return resource == null ? string.Empty : resource.ResourcePath ?? string.Empty;
	}

	private static T? LoadResourceOrNull<T>(string? path) where T : Resource
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return null;
		}

		Resource? resource = ResourceLoader.Load(path);
		return resource as T;
	}

	public void SetPlayerCredits(int credits)
	{
		PlayerCredits = Mathf.Max(0, credits);
		SaveGameToDisk();
	}

	public void AddPlayerCredits(int amount)
	{
		SetPlayerCredits(PlayerCredits + amount);
	}

	public void ClearLastExtractionLootValue()
	{
		LastExtractionLootValue = 0;
	}

	public bool HasSecureContainerUsage()
	{
		return GetRemainingSecureContainerRentalRuns() > 0
			|| GetRemainingSecureContainerQuota() > 0;
	}

	public int GetRemainingSecureContainerRentalRuns()
	{
		return GetRemainingSecureContainerRentalRuns(DefaultSecureContainerId);
	}

	public int GetRemainingSecureContainerQuota()
	{
		return GetRemainingSecureContainerQuota(DefaultSecureContainerId);
	}

	public int GetSecureContainerPendingBill()
	{
		return GetSecureContainerPendingBill(DefaultSecureContainerId);
	}

	public float GetSecureContainerCurrentGreedRate()
	{
		return GetSecureContainerCurrentGreedRate(DefaultSecureContainerId);
	}

	public int GetSecureContainerUnusedRunCount()
	{
		return GetSecureContainerUnusedRunCount(DefaultSecureContainerId);
	}

	public IReadOnlyList<SecureContainerBillEntryView> GetSecureContainerPendingBillEntries()
	{
		return GetSecureContainerPendingBillEntries(DefaultSecureContainerId);
	}

	public int GetRemainingSecureContainerRentalRuns(string containerId)
	{
		return Mathf.Max(0, GetOrCreateSecureContainerState(containerId).RentalUsesRemaining);
	}

	public int GetRemainingSecureContainerQuota(string containerId)
	{
		return Mathf.Max(0, GetOrCreateSecureContainerState(containerId).ChallengeQuotaRemaining);
	}

	public int GetSecureContainerPendingBill(string containerId)
	{
		return Mathf.Max(0, GetOrCreateSecureContainerState(containerId).PendingBill);
	}

	public float GetSecureContainerCurrentGreedRate(string containerId)
	{
		SecureContainerRuntimeState state = GetOrCreateSecureContainerState(containerId);
		return Mathf.Max(state.BaseGreedRate, state.CurrentGreedRate);
	}

	public int GetSecureContainerUnusedRunCount(string containerId)
	{
		return Mathf.Max(0, GetOrCreateSecureContainerState(containerId).UnusedRunCount);
	}

	public IReadOnlyList<SecureContainerBillEntryView> GetSecureContainerPendingBillEntries(string containerId)
	{
		SecureContainerRuntimeState state = GetOrCreateSecureContainerState(containerId);
		List<SecureContainerBillEntryView> entries = [];
		foreach (SecureContainerBillEntry entry in state.PendingBillEntries)
		{
			entries.Add(new SecureContainerBillEntryView(entry.DisplayName, entry.AssessedValue, entry.FeeCharged));
		}

		return entries;
	}

	public bool TryPurchaseSecureContainerRentalInsurance(out int chargedCredits)
	{
		SecureContainerRuntimeState state = DefaultSecureContainerState;
		chargedCredits = SecureContainerRentalCost;
		if (PlayerCredits < chargedCredits)
		{
			return false;
		}

		AddPlayerCredits(-chargedCredits);
		state.RentalUsesRemaining += Mathf.Max(1, SecureContainerRentalUsesPerPurchase);
		RefreshSecureContainerInsuranceMode();
		SaveGameToDisk();
		return true;
	}

	public void RegisterSecureContainer(GridContainerDefinition? definition)
	{
		if (definition == null)
		{
			return;
		}

		SecureContainerRuntimeState state = GetOrCreateSecureContainerState(definition.ContainerId);
		state.ContainerId = string.IsNullOrWhiteSpace(definition.ContainerId) ? DefaultSecureContainerId : definition.ContainerId;
		state.BaseGreedRate = definition.SecureBaseGreedRateOverride >= 0.0f
			? definition.SecureBaseGreedRateOverride
			: Mathf.Max(0.0f, SecureContainerBaseGreedRate);
		state.CurrentGreedRate = Mathf.Max(state.BaseGreedRate, state.CurrentGreedRate);
	}

	public bool GrantSecureContainerQuotaFromChallenge(int quotaAmount)
	{
		SecureContainerRuntimeState state = DefaultSecureContainerState;
		int grantedAmount = Mathf.Max(0, quotaAmount);
		if (grantedAmount <= 0)
		{
			return false;
		}

		state.ChallengeQuotaRemaining = Mathf.Max(0, state.ChallengeQuotaRemaining) + grantedAmount;
		RefreshSecureContainerInsuranceMode();
		SaveGameToDisk();
		return true;
	}

	public bool CanStoreInSecureContainer(ItemInstance? item, out SecureContainerStoreFailureReason failureReason)
	{
		return CanStoreInSecureContainer(DefaultSecureContainerId, item, out failureReason);
	}

	public bool CanStoreInSecureContainer(string containerId, ItemInstance? item, out SecureContainerStoreFailureReason failureReason)
	{
		SecureContainerRuntimeState state = GetOrCreateSecureContainerState(containerId);
		failureReason = SecureContainerStoreFailureReason.None;
		if (item == null)
		{
			return false;
		}

		if (Mathf.Max(0, state.RentalUsesRemaining) > 0)
		{
			return true;
		}

		if (Mathf.Max(0, state.ChallengeQuotaRemaining) > 0)
		{
			return true;
		}

		if (state.PendingBill > 0)
		{
			failureReason = SecureContainerStoreFailureReason.RentalExpired;
			return false;
		}

		if (state.RentalUsesRemaining <= 0 && state.ChallengeQuotaRemaining <= 0 && (state.CumulativeFeeCharged > 0 || state.UnusedRunCount > 0))
		{
			failureReason = SecureContainerStoreFailureReason.RentalExpired;
			return false;
		}

		if (state.ChallengeQuotaRemaining <= 0 && state.CumulativeFeeCharged > 0)
		{
			failureReason = SecureContainerStoreFailureReason.InsufficientQuota;
			return false;
		}

		if (SecureContainerInsuranceMode == SecureContainerInsuranceMode.None)
		{
			failureReason = SecureContainerStoreFailureReason.NoInsurance;
			return false;
		}

		failureReason = SecureContainerStoreFailureReason.NoInsurance;
		return false;
	}

	public void NotifySecureContainerItemStored(ItemInstance? item)
	{
		NotifySecureContainerItemStored(DefaultSecureContainerId, item);
	}

	public void NotifySecureContainerItemStored(string containerId, ItemInstance? item)
	{
		if (item == null)
		{
			return;
		}

		NotifySecureContainerValueStored(containerId, item.InstanceId, item.Definition.DisplayName, Items.ItemValueClassifier.GetMarketValue(item));
	}

	public void NotifySecureContainerValueStored(string instanceId, string displayName, int marketValue)
	{
		NotifySecureContainerValueStored(DefaultSecureContainerId, instanceId, displayName, marketValue);
	}

	public void NotifySecureContainerValueStored(string containerId, string instanceId, string displayName, int marketValue)
	{
		SecureContainerRuntimeState state = GetOrCreateSecureContainerState(containerId);
		if (string.IsNullOrWhiteSpace(instanceId))
		{
			return;
		}

		if (!state.RecordedItemIdsThisRun.Add(instanceId))
		{
			return;
		}

		state.UsedThisRun = true;
		state.CurrentRunRecordedValue += Mathf.Max(0, marketValue);
		state.CurrentRunBillEntries.Add(new SecureContainerBillEntry(
			instanceId,
			string.IsNullOrWhiteSpace(displayName) ? instanceId : displayName,
			Mathf.Max(0, marketValue),
			0));
	}

	public bool TrySettleSecureContainerBill(out int chargedCredits)
	{
		return TrySettleSecureContainerBill(DefaultSecureContainerId, out chargedCredits);
	}

	public bool TrySettleSecureContainerBill(string containerId, out int chargedCredits)
	{
		SecureContainerRuntimeState state = GetOrCreateSecureContainerState(containerId);
		chargedCredits = Mathf.Max(0, state.PendingBill);
		if (chargedCredits <= 0)
		{
			return true;
		}

		if (PlayerCredits < chargedCredits)
		{
			return false;
		}

		AddPlayerCredits(-chargedCredits);
		state.PendingBill = 0;
		state.PendingBillEntries.Clear();
		SaveGameToDisk();
		return true;
	}

	private static void NormalizeOwnedItemsAsBase(Node3D? player)
	{
		if (player == null)
		{
			return;
		}

		NormalizeContainerItemsAsBase(GetPlayerInventoryContainer(player));
		NormalizeContainerItemsAsBase(GetPlayerSecureContainer(player));

		EquipmentComponent? equipment = GetPlayerEquipment(player);
		if (equipment == null)
		{
			return;
		}

		foreach ((EquipmentSlotType _, EquipmentSlotRecord slot) in equipment.Slots)
		{
			slot.EquippedItem?.SetAcquisitionState(ItemAcquisitionState.Base);
		}
	}

	private static int GetCurrentRunLootValue(Node3D? player)
	{
		if (player == null)
		{
			return 0;
		}

		int total = 0;
		total += SumRunLootValue(GetPlayerInventoryContainer(player));
		total += SumRunLootValue(GetPlayerSecureContainer(player));

		EquipmentComponent? equipment = GetPlayerEquipment(player);
		if (equipment != null)
		{
			foreach ((EquipmentSlotType _, EquipmentSlotRecord slot) in equipment.Slots)
			{
				if (slot.EquippedItem?.CountsAsRunLoot == true)
				{
					total += Items.ItemValueClassifier.GetRunLootMarketValue(slot.EquippedItem);
				}
			}
		}

		return total;
	}

	private static int SumRunLootValue(GridContainerComponent? container)
	{
		if (container == null)
		{
			return 0;
		}

		int total = 0;
		foreach (ContainerItemRecord record in container.ItemRecords)
		{
			if (!record.Item.CountsAsRunLoot)
			{
				continue;
			}

			total += Items.ItemValueClassifier.GetRunLootMarketValue(record.Item);
		}

		return total;
	}

	private static void NormalizeContainerItemsAsBase(GridContainerComponent? container)
	{
		if (container == null)
		{
			return;
		}

		foreach (ContainerItemRecord record in container.ItemRecords)
		{
			record.Item.SetAcquisitionState(ItemAcquisitionState.Base);
		}
	}

	private void InitializeSecureContainerState()
	{
		SecureContainerRuntimeState state = DefaultSecureContainerState;
		state.ContainerId = DefaultSecureContainerId;
		state.BaseGreedRate = Mathf.Max(0.0f, SecureContainerBaseGreedRate);
		state.CurrentGreedRate = Mathf.Max(state.BaseGreedRate, state.CurrentGreedRate);
		RefreshSecureContainerInsuranceMode();
	}

	private void BeginSecureContainerRun()
	{
		foreach (SecureContainerRuntimeState state in _secureContainerStates.Values)
		{
			state.RecordedItemIdsThisRun.Clear();
			state.CurrentRunRecordedValue = 0;
			state.CurrentRunBillEntries.Clear();
			state.UsedThisRun = false;
		}
	}

	private void FinalizeSecureContainerRun()
	{
		if (!HasActiveRun)
		{
			BeginSecureContainerRun();
			return;
		}

		foreach (SecureContainerRuntimeState state in _secureContainerStates.Values)
		{
			if (state.UsedThisRun && state.CurrentRunRecordedValue > 0)
			{
				int assessedValue = Mathf.Max(0, state.CurrentRunRecordedValue);
				float greedRate = Mathf.Max(state.BaseGreedRate, state.CurrentGreedRate);
				int fee = Mathf.RoundToInt(assessedValue * greedRate);
				int previousCumulativeFee = state.CumulativeFeeCharged;

				if (state.RentalUsesRemaining > 0)
				{
					state.RentalUsesRemaining--;
					state.PendingBill += fee;
				}
				else if (state.ChallengeQuotaRemaining > 0)
				{
					int coveredAmount = Mathf.Min(state.ChallengeQuotaRemaining, fee);
					state.ChallengeQuotaRemaining -= coveredAmount;
					state.PendingBill += Mathf.Max(0, fee - coveredAmount);
				}
				else
				{
					state.PendingBill += fee;
				}

				state.CumulativeFeeCharged += fee;
				AppendPendingBillEntries(state, greedRate);
				int previousThreshold = previousCumulativeFee / Mathf.Max(1, SecureContainerGreedThresholdValue);
				int currentThreshold = state.CumulativeFeeCharged / Mathf.Max(1, SecureContainerGreedThresholdValue);
				int thresholdDelta = Mathf.Max(0, currentThreshold - previousThreshold);
				if (thresholdDelta > 0)
				{
					state.CurrentGreedRate += thresholdDelta * Mathf.Max(0.0f, SecureContainerGreedIncreasePerThreshold);
				}

				state.UnusedRunCount = 0;
			}
			else
			{
				state.UnusedRunCount++;
				state.CurrentGreedRate = Mathf.Max(
					state.BaseGreedRate,
					state.CurrentGreedRate - Mathf.Max(0.0f, SecureContainerGreedDecayPerUnusedRun));
			}
		}

		BeginSecureContainerRun();
		RefreshSecureContainerInsuranceMode();
	}

	private void RefreshSecureContainerInsuranceMode()
	{
		if (_secureContainerStates.Values.Any(state => state.RentalUsesRemaining > 0))
		{
			SecureContainerInsuranceMode = SecureContainerInsuranceMode.Rental;
			return;
		}

		if (_secureContainerStates.Values.Any(state => state.ChallengeQuotaRemaining > 0))
		{
			SecureContainerInsuranceMode = SecureContainerInsuranceMode.Quota;
			return;
		}

		SecureContainerInsuranceMode = SecureContainerInsuranceMode.None;
	}

	private List<ContainerItemSnapshot> GetOrCreateSecureContainerSnapshot(string containerId)
	{
		string resolvedId = string.IsNullOrWhiteSpace(containerId) ? DefaultSecureContainerId : containerId;
		if (!_secureContainerSnapshots.TryGetValue(resolvedId, out List<ContainerItemSnapshot>? snapshot))
		{
			snapshot = [];
			_secureContainerSnapshots[resolvedId] = snapshot;
		}

		return snapshot;
	}

	private SecureContainerRuntimeState GetOrCreateSecureContainerState(string containerId)
	{
		string resolvedId = string.IsNullOrWhiteSpace(containerId) ? DefaultSecureContainerId : containerId;
		if (!_secureContainerStates.TryGetValue(resolvedId, out SecureContainerRuntimeState? state))
		{
			state = new SecureContainerRuntimeState
			{
				ContainerId = resolvedId,
				BaseGreedRate = Mathf.Max(0.0f, SecureContainerBaseGreedRate),
				CurrentGreedRate = Mathf.Max(0.0f, SecureContainerBaseGreedRate),
			};
			_secureContainerStates[resolvedId] = state;
		}

		return state;
	}

	private static string ResolveSecureContainerId(GridContainerComponent? container)
	{
		return container?.Definition?.ContainerId ?? DefaultSecureContainerId;
	}

	private void AppendPendingBillEntries(SecureContainerRuntimeState state, float greedRate)
	{
		foreach (SecureContainerBillEntry entry in state.CurrentRunBillEntries)
		{
			int entryFee = Mathf.RoundToInt(entry.AssessedValue * greedRate);
			state.PendingBillEntries.Add(entry with { FeeCharged = entryFee });
		}
	}

	public readonly record struct SecureContainerBillEntryView(
		string DisplayName,
		int AssessedValue,
		int FeeCharged);

	private readonly record struct SecureContainerBillEntry(
		string SourceInstanceId,
		string DisplayName,
		int AssessedValue,
		int FeeCharged);

	private sealed class SaveGameFile
	{
		public int Version { get; set; }
		public Dictionary<string, JsonElement> Sections { get; set; } = [];
	}

	private sealed class SaveGameData
	{
		public int Version { get; set; }
		public bool HasActiveRun { get; set; }
		public bool EliteDefeated { get; set; }
		public bool ExtractionUnlocked { get; set; }
		public int PlayerCredits { get; set; }
		public int LastExtractionLootValue { get; set; }
		public bool PlayerStateInitialized { get; set; }
		public List<ContainerItemSaveData> PlayerInventory { get; set; } = [];
		public List<EquipmentItemSaveData> Equipment { get; set; } = [];
		public List<ContainerItemSaveData> Warehouse { get; set; } = [];
		public Dictionary<string, List<ContainerItemSaveData>> SecureContainers { get; set; } = [];
		public List<string> LearnedWeaponSkillPaths { get; set; } = [];
		public Dictionary<string, SecureContainerStateSaveData> SecureContainerStates { get; set; } = [];
	}

	private sealed class ContainerItemSaveData
	{
		public int OriginX { get; set; }
		public int OriginY { get; set; }
		public ItemSaveData Item { get; set; } = new();
	}

	private sealed class EquipmentItemSaveData
	{
		public int SlotType { get; set; }
		public ItemSaveData Item { get; set; } = new();
	}

	private sealed class ItemSaveData
	{
		public string DefinitionPath { get; set; } = string.Empty;
		public int StackCount { get; set; } = 1;
		public bool IsRotated { get; set; }
		public int RemainingUses { get; set; }
		public float CurrentDurability { get; set; }
		public float CurrentMaxDurability { get; set; }
		public float CurrentArmorPoint { get; set; }
		public float CurrentMaxArmorPoint { get; set; }
		public int CurrentMagazineAmmo { get; set; }
		public string LoadedAmmoItemId { get; set; } = string.Empty;
		public int LoadedAmmoType { get; set; }
		public int LoadedAmmoTier { get; set; }
		public float LoadedAmmoPenetrationTier { get; set; }
		public int AcquisitionState { get; set; }
		public int RunLootStackCount { get; set; }
		public List<WeaponSkillSlotSaveData> WeaponSkillSlots { get; set; } = [];
	}

	private sealed class WeaponSkillSlotSaveData
	{
		public int SlotIndex { get; set; }
		public string SkillPath { get; set; } = string.Empty;
		public string SourceItemDefinitionPath { get; set; } = string.Empty;
		public int SourceAcquisitionState { get; set; }
		public int RemainingUses { get; set; }
		public bool ConsumeUsesOnCast { get; set; }
		public bool PermanentlyUnlocked { get; set; }
		public bool CanBeLearned { get; set; }
		public int UnlockConditionType { get; set; }
		public int RequiredUseCount { get; set; }
		public int RequiredKillCount { get; set; }
		public int RequiredBossKillCount { get; set; }
		public int CurrentUseCount { get; set; }
		public int CurrentKillCount { get; set; }
		public int CurrentBossKillCount { get; set; }
	}

	private sealed class SecureContainerStateSaveData
	{
		public string ContainerId { get; set; } = DefaultSecureContainerId;
		public int RentalUsesRemaining { get; set; }
		public int ChallengeQuotaRemaining { get; set; }
		public int PendingBill { get; set; }
		public int CumulativeFeeCharged { get; set; }
		public int CurrentRunRecordedValue { get; set; }
		public int UnusedRunCount { get; set; }
		public float BaseGreedRate { get; set; }
		public float CurrentGreedRate { get; set; }
		public bool UsedThisRun { get; set; }
		public List<string> RecordedItemIdsThisRun { get; set; } = [];
		public List<BillEntrySaveData> CurrentRunBillEntries { get; set; } = [];
		public List<BillEntrySaveData> PendingBillEntries { get; set; } = [];
	}

	private sealed class BillEntrySaveData
	{
		public string SourceInstanceId { get; set; } = string.Empty;
		public string DisplayName { get; set; } = string.Empty;
		public int AssessedValue { get; set; }
		public int FeeCharged { get; set; }
	}

	private sealed class SecureContainerRuntimeState
	{
		public string ContainerId { get; set; } = DefaultSecureContainerId;
		public int RentalUsesRemaining { get; set; }
		public int ChallengeQuotaRemaining { get; set; }
		public int PendingBill { get; set; }
		public int CumulativeFeeCharged { get; set; }
		public int CurrentRunRecordedValue { get; set; }
		public int UnusedRunCount { get; set; }
		public float BaseGreedRate { get; set; }
		public float CurrentGreedRate { get; set; }
		public bool UsedThisRun { get; set; }
		public HashSet<string> RecordedItemIdsThisRun { get; } = [];
		public List<SecureContainerBillEntry> CurrentRunBillEntries { get; } = [];
		public List<SecureContainerBillEntry> PendingBillEntries { get; } = [];
	}
}
