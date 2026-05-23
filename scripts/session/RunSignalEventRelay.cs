using Godot;

namespace BattleHarvesterStudy.Session;

// Run (游戏运行、一次会话) Signal (信号) Event (事件) Relay (中继、接力)
// 整体翻译: 游戏会话信号事件中继器 / 运行信号事件转接器
// 功能: 该类用于将一个Godot节点的特定信号转换为RunEventHub的通用事件。它作为一个中介，将具体的、低层级的信号抽象为高层级的游戏事件，实现模块间解耦。
public partial class RunSignalEventRelay : Node
{
	// Source (来源) Path (路径)
	// 功能: 用于在编辑器中指定要监听信号的源节点路径。
	[Export]
	public NodePath SourcePath { get; set; } = new();

	// Source (来源) Signal (信号) Name (名称)
	// 功能: 用于在编辑器中指定要监听的源信号的名称。
	[Export]
	public StringName SourceSignalName { get; set; } = new();

	// Event (事件) Hub (中心) Path (路径)
	// 功能: RunEventHub的节点路径，RunEventHub用于发布转换后的事件。
	[Export]
	public NodePath EventHubPath { get; set; } = new("../RunEventHub");

	// Event (事件) Id (标识)
	// 功能: 当源信号被触发时，要通过RunEventHub发布的事件的唯一标识符。
	[Export]
	public string EventId { get; set; } = string.Empty;

	// Scope (作用域) Id (标识)
	// 功能: 附加到事件的作用域标识符，用于区分事件的上下文（例如，来自哪个房间的事件）。
	[Export]
	public string ScopeId { get; set; } = string.Empty;

	// Fire (触发) Once (一次)
	// 功能: 一个布尔值，如果为true，则表示这个中继器在第一次触发事件后将不再响应后续的源信号。
	[Export]
	public bool FireOnce { get; set; }

	// 内部变量
	// _source: 用于存储通过SourcePath获取到的源节点的引用。
	private Node? _source;
	// _eventHub: 用于存储通过EventHubPath获取到的RunEventHub的引用。
	private RunEventHub? _eventHub;
	// _hasFired: 一个内部布尔标记，用于记录当FireOnce为true时，事件是否已经触发过。
	private bool _hasFired;

	// _Ready (就绪)
	// 功能: Godot节点的生命周期方法，当节点及其所有子节点被添加到场景树并准备好时，Godot会自动调用它。
	public override void _Ready()
	{
		// Get (获取) Node (节点) Or (或者) Null (空)
		// 整体翻译: 获取节点或返回空
		// 功能: 尝试根据指定的节点路径获取一个节点。如果节点存在且类型匹配，则返回该节点；否则返回 null。
		// 这里用于根据 SourcePath 获取源节点的引用。
		_source = GetNodeOrNull<Node>(SourcePath);
		// Get (获取) Node (节点) Or (或者) Null (空)
		// 整体翻译: 获取节点或返回空
		// 功能: 尝试根据指定的节点路径获取一个节点。如果节点存在且类型匹配，则返回该节点；否则返回 null。
		// 这里用于根据 EventHubPath 获取 RunEventHub 的引用。
		_eventHub = GetNodeOrNull<RunEventHub>(EventHubPath);

		// Source (来源) Signal (信号) Name (名称) Is (是) Empty (空的)
		// 整体翻译: 源信号名称是否为空
		// 功能: 检查 SourceSignalName 属性是否为空。如果为空，表示没有指定要监听的信号。
		// 检查必要的引用和信号名称是否已设置。如果缺失，则不进行连接，直接返回，避免运行时错误。
		if (_source == null || _eventHub == null || SourceSignalName.IsEmpty)
		{
			return;
		}

		// Connect (连接)
		// 整体翻译: 连接
		// 功能: 将一个信号连接到指定的方法。当信号被发出时，连接的方法将被调用。
		// Callable (可调用) From (来自)
		// 整体翻译: 从可调用对象创建
		// 功能: 将一个方法（OnSourceTriggered）封装成一个 Callable 对象，使其可以被 Godot 的信号连接机制使用。
		// 将源节点（_source）发出的特定信号（SourceSignalName）连接到本脚本的 OnSourceTriggered 方法。这意味着当源信号被触发时，OnSourceTriggered 方法会被自动调用。
		_source.Connect(SourceSignalName, Callable.From(OnSourceTriggered));
	}

	// OnSourceTriggered (在源触发时)
	// 功能: 当源信号被触发时调用的回调方法。它负责将接收到的具体信号转换为抽象事件并通过RunEventHub发布。
	private void OnSourceTriggered()
	{
		// _hasFired (已触发): FireOnce (触发一次)
		// 功能: 如果FireOnce设置为true，并且事件已经触发过一次，则不再处理，直接返回。
		if (_hasFired && FireOnce)
		{
			return;
		}

		// _hasFired = true: 标记事件已触发，用于FireOnce功能。
		_hasFired = true;
		// RaiseEvent (引发事件): EventId (事件标识): ScopeId (作用域标识)
		// 功能: 通过RunEventHub的RaiseEvent方法，发布一个带有EventId和ScopeId的抽象游戏事件。
		_eventHub?.RaiseEvent(EventId, ScopeId);
	}
}
