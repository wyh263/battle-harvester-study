using Godot;

namespace BattleHarvesterStudy.Session;

// Extraction (撤离) Controller (控制器)
// 整体翻译: 撤离控制器
// 功能: 该脚本负责管理游戏中的撤离功能，控制玩家何时可以进行撤离。它监听特定的游戏事件（如 Boss 被击败），
// 以决定是否解锁撤离。当撤离状态改变时，它会发出信号通知其他系统。
public partial class ExtractionController : Node
{
	// Signal (信号)
	// 功能: C# 特性，用于标记一个委托为 Godot 信号。当信号被 EmitSignal 方法触发时，所有连接到该信号的监听器都会收到通知。
	[Signal]
	// Availability (可用性) Changed (改变) Event (事件) Handler (处理者)
	// 整体翻译: 可用性改变事件处理委托
	// 功能: 定义了一个委托类型，用于处理 AvailabilityChanged 信号。当撤离可用性状态改变时，此信号会被触发，并带有一个布尔参数 isAvailable。
	public delegate void AvailabilityChangedEventHandler(bool isAvailable);

	// Event (事件) Hub (中心) Path (路径)
	// 功能: 导出变量，用于指定场景中 RunEventHub 的节点路径，用于订阅游戏事件。
	[Export]
	public NodePath EventHubPath { get; set; } = new("../RunEventHub");

	// Unlock (解锁) On (在) Events (事件)
	// 功能: 导出变量，一个字符串数组，列出了当这些事件发生时，撤离功能将被解锁。例如，"boss_defeated"。
	[Export]
	public Godot.Collections.Array<string> UnlockOnEvents { get; set; } = ["boss_defeated"];

	// Is (是) Available (可用的)
	// 功能: 表示当前撤离功能是否可用（即玩家是否可以撤离）。
	public bool IsAvailable { get; private set; }

	// _event (事件) Hub (中心)
	// 功能: 私有字段，存储 RunEventHub 实例的引用。
	private RunEventHub? _eventHub;

	public override void _Ready()
	{
		// Get (获取) Node (节点) Or (或者) Null (空)
		// 整体翻译: 获取节点或返回空
		// 功能: 尝试根据指定的节点路径获取一个节点。如果节点存在且类型匹配，则返回该节点；否则返回 null。
		// 这里用于获取 _eventHub 的引用。
		_eventHub = GetNodeOrNull<RunEventHub>(EventHubPath);
		if (_eventHub != null)
		{
			// Event (事件) Raised (触发)
			// 整体翻译: 事件触发
			// 功能: 订阅 _eventHub 的 EventRaised 信号，当有事件触发时调用 OnEventRaised 方法。
			_eventHub.EventRaised += OnEventRaised;
		}

		// Run (运行、会话) Session (会话) Instance (实例) Extraction (撤离) Unlocked (解锁)
		// 整体翻译: 运行会话实例撤离已解锁
		// 功能: 访问 RunSession 单例的 ExtractionUnlocked 属性，获取当前撤离是否已解锁的状态。使用 null 合并运算符 ?? 提供默认值 false。
		SetAvailable(RunSession.Instance?.ExtractionUnlocked ?? false);
	}

	public override void _ExitTree()
	{
		if (_eventHub != null)
		{
			// Event (事件) Raised (触发)
			// 整体翻译: 事件触发
			// 功能: 取消订阅 _eventHub 的 EventRaised 信号，防止内存泄漏。
			_eventHub.EventRaised -= OnEventRaised;
		}
	}

	/// Unlock (解锁)
/// 整体翻译: 解锁
/// 功能: 解锁撤离功能。该方法会调用 RunSession.Instance?.MarkEliteDefeated() 标记精英敌人已被击败，并设置撤离为可用状态。
	public void Unlock()
	{
		// Run (运行、会话) Session (会话) Instance (实例) Mark (标记) Elite (精英) Defeated (击败)
		// 整体翻译: 运行会话实例标记精英敌人击败
		// 功能: 调用 RunSession 单例的 MarkEliteDefeated 方法，通知游戏会话精英敌人已被击败，这通常会触发后续的游戏逻辑，例如保存状态或更新任务。
		RunSession.Instance?.MarkEliteDefeated();
		// Set (设置) Available (可用)
		// 整体翻译: 设置可用
		// 功能: 调用 SetAvailable 方法，将撤离状态设置为 true，表示撤离现在可用。
		SetAvailable(true);
	}

	/// Reset (重置) State (状态)
/// 整体翻译: 重置状态
/// 功能: 重置撤离控制器的状态，将撤离设置为不可用。
	public void ResetState()
	{
		// Set (设置) Available (可用)
		// 整体翻译: 设置可用
		// 功能: 调用 SetAvailable 方法，将撤离状态设置为 false，表示撤离现在不可用。
		SetAvailable(false);
	}

	/// On (在) Event (事件) Raised (触发)
/// 整体翻译: 在事件触发时
/// 功能: 当 RunEventHub 触发任何事件时，此方法会被调用。它检查触发的事件是否在 UnlockOnEvents 列表中，
/// 如果是，则调用 Unlock 方法解锁撤离。
	private void OnEventRaised(string eventId, string _scopeId)
	{
		// Unlock (解锁) On (在) Events (事件) Contains (包含)
		// 整体翻译: 解锁事件是否包含
		// 功能: 检查 UnlockOnEvents 数组是否包含当前触发的 eventId。如果包含，说明这个事件是解锁撤离的条件之一。
		if (UnlockOnEvents.Contains(eventId))
		{
			// Unlock (解锁)
			// 整体翻译: 解锁
			// 功能: 调用 Unlock 方法，解锁撤离功能。
			Unlock();
		}
	}

	/// Set (设置) Available (可用)
/// 整体翻译: 设置可用
/// 功能: 设置撤离功能的可用状态。如果新的状态与当前状态不同，则更新 IsAvailable 属性，
/// 并触发 AvailabilityChanged 信号，通知所有监听者撤离状态已改变。
	private void SetAvailable(bool isAvailable)
	{
		// Is (是) Available (可用) == (等于)
		// 整体翻译: 是否可用等于
		// 功能: 检查新的可用状态 isAvailable 是否与当前状态 IsAvailable 相同。如果相同，则无需更新，直接返回。
		if (IsAvailable == isAvailable)
		{
			// Return (返回)
			// 整体翻译: 返回
			// 功能: 退出方法。
			return;
		}

		IsAvailable = isAvailable;
		// Emit (发射) Signal (信号) Signal (信号) Name (名称) Availability (可用性) Changed (改变)
		// 整体翻译: 发射信号信号名称可用性改变
		// 功能: 触发 AvailabilityChanged 信号，并将新的可用状态 IsAvailable 作为参数传递。
		EmitSignal(SignalName.AvailabilityChanged, IsAvailable);
	}
}
