using Godot;

namespace BattleHarvesterStudy.Session;

// Run (运行、会话) Map (地图) Controller (控制器)
// 整体翻译: 游戏运行地图控制器
// 功能: 该脚本负责管理战斗地图场景的整体生命周期和游戏逻辑。
// 它监听玩家和精英敌人的生命状态，并在玩家死亡或精英敌人被击败时触发相应的游戏事件或流程（如游戏失败、解锁撤离）。
// 它还负责初始化 ExtractionController 和 RunEventHub，确保它们在地图场景中正常工作。
public partial class RunMapController : Node
{
	// Player (玩家) Health (生命) Path (路径)
	// 功能: 导出变量，用于指定场景中玩家 HealthComponent 的节点路径。
	[Export]
	public NodePath PlayerHealthPath { get; set; } = new("../Player/Components/Health");

	// Elite (精英) Health (生命) Path (路径)
	// 功能: 导出变量，用于指定场景中精英敌人（Boss）HealthComponent 的节点路径。
	[Export]
	public NodePath EliteHealthPath { get; set; } = new();

	// Extraction (撤离) Controller (控制器) Path (路径)
	// 功能: 导出变量，用于指定场景中 ExtractionController 的节点路径，用于管理撤离功能。
	[Export]
	public NodePath ExtractionControllerPath { get; set; } = new("../ExtractionController");

	// Event (事件) Hub (中心) Path (路径)
	// 功能: 导出变量，用于指定场景中 RunEventHub 的节点路径，用于发布和订阅游戏事件。
	[Export]
	public NodePath EventHubPath { get; set; } = new("../RunEventHub");

	// Elite (精英) Defeated (击败) Event (事件) Id (ID)
	// 功能: 导出变量，定义精英敌人被击败时要发布的事件 ID，默认为 "boss_defeated"。
	[Export]
	public string EliteDefeatedEventId { get; set; } = "boss_defeated";

	// Return (返回) Home (家) Delay (延迟) Seconds (秒)
	// 功能: 导出变量，定义玩家死亡后返回基地场景的延迟时间（秒）。
	[Export(PropertyHint.Range, "0,5,0.1")]
	public float ReturnHomeDelaySeconds { get; set; } = 0.4f;

	// _player (玩家) Health (生命)
	// 功能: 私有字段，存储玩家 HealthComponent 实例的引用。
	private HealthComponent? _playerHealth;

	// _elite (精英) Health (生命)
	// 功能: 私有字段，存储精英敌人 HealthComponent 实例的引用。
	private HealthComponent? _eliteHealth;

	// _extraction (撤离) Controller (控制器)
	// 功能: 私有字段，存储 ExtractionController 实例的引用。
	private ExtractionController? _extractionController;

	// _event (事件) Hub (中心)
	// 功能: 私有字段，存储 RunEventHub 实例的引用。y
	private RunEventHub? _eventHub;

	public override void _Ready()
	{
		// Get (获取) Node (节点) Or (或者) Null (空)
		// 整体翻译: 获取节点或返回空
		// 功能: 尝试根据指定的节点路径获取一个节点。如果节点存在且类型匹配，则返回该节点；否则返回 null。
		// 这里用于获取 _playerHealth 的引用。
		_playerHealth = GetNodeOrNull<HealthComponent>(PlayerHealthPath);
		// Get (获取) Node (节点) Or (或者) Null (空)
		// 整体翻译: 获取节点或返回空
		// 功能: 尝试根据指定的节点路径获取一个节点。如果节点存在且类型匹配，则返回该节点；否则返回 null。
		// 这里用于获取 _eliteHealth 的引用。
		_eliteHealth = GetNodeOrNull<HealthComponent>(EliteHealthPath);
		// Get (获取) Node (节点) Or (或者) Null (空)
		// 整体翻译: 获取节点或返回空
		// 功能: 尝试根据指定的节点路径获取一个节点。如果节点存在且类型匹配，则返回该节点；否则返回 null。
		// 这里用于获取 _extractionController 的引用。
		_extractionController = GetNodeOrNull<ExtractionController>(ExtractionControllerPath);
		// Get (获取) Node (节点) Or (或者) Null (空)
		// 整体翻译: 获取节点或返回空
		// 功能: 尝试根据指定的节点路径获取一个节点。如果节点存在且类型匹配，则返回该节点；否则返回 null。
		// 这里用于获取 _eventHub 的引用。
		_eventHub = GetNodeOrNull<RunEventHub>(EventHubPath);
		_extractionController?.ResetState();
		if (_playerHealth != null)
		{
			// Died (死亡) Occurred (发生)
			// 整体翻译: 死亡发生
			// 功能: 订阅玩家 HealthComponent 的 DiedOccurred 信号，当玩家死亡时触发 OnPlayerDied 方法。
			_playerHealth.DiedOccurred += OnPlayerDied;
		}

		if (_eliteHealth != null)
		{
			// Died (死亡) Occurred (发生)
			// 整体翻译: 死亡发生
			// 功能: 订阅精英敌人 HealthComponent 的 DiedOccurred 信号，当精英敌人死亡时触发 OnEliteDied 方法。
			_eliteHealth.DiedOccurred += OnEliteDied;
		}
	}

	public override void _ExitTree()
	{
		if (_playerHealth != null)
		{
			// Died (死亡) Occurred (发生)
			// 整体翻译: 死亡发生
			// 功能: 取消订阅玩家 HealthComponent 的 DiedOccurred 信号，防止内存泄漏。
			_playerHealth.DiedOccurred -= OnPlayerDied;
		}

		if (_eliteHealth != null)
		{
			// Died (死亡) Occurred (发生)
			// 整体翻译: 死亡发生
			// 功能: 取消订阅精英敌人 HealthComponent 的 DiedOccurred 信号，防止内存泄漏。
			_eliteHealth.DiedOccurred -= OnEliteDied;
		}
	}

	private async void OnPlayerDied()
	{
		// Get (获取) Tree (树)
		// 整体翻译: 获取场景树
		// 功能: 获取当前节点所属的 SceneTree 对象。
		// Create (创建) Timer (计时器)
		// 整体翻译: 创建计时器
		// 功能: 在场景树中创建一个 SceneTreeTimer，用于在指定秒数后发出 Timeout 信号。
		SceneTreeTimer timer = GetTree().CreateTimer(ReturnHomeDelaySeconds);
		// To (到) Signal (信号)
		// 整体翻译: 到信号
		// 功能: 将一个信号转换为可等待的任务，通常与 await 关键字一起使用，等待信号发出。
		// Signal (信号) Name (名称) Timeout (超时)
		// 整体翻译: 信号名称超时
		// 功能: Godot C# 的一个特性，提供类型安全的字符串常量，代表 SceneTreeTimer 的 Timeout 信号名称。
		await ToSignal(timer, SceneTreeTimer.SignalName.Timeout);
		RunSession.Instance?.FailRunAndReturnHome();
	}

	private void OnEliteDied()
	{
		// Raise (触发) Event (事件)
		// 整体翻译: 触发事件
		// 功能: 调用 _eventHub 的 RaiseEvent 方法，发布 EliteDefeatedEventId 事件，通知所有订阅者精英敌人已被击败。
		_eventHub?.RaiseEvent(EliteDefeatedEventId);
	}
}
