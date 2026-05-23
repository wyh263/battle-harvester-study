using Godot;

namespace BattleHarvesterStudy.Session;

// Run (运行、会话) Event (事件) Hub (中心)
// 整体翻译: 游戏运行事件中心
// 功能: 该类作为游戏内所有事件的中央分发器，采用单例模式。它允许不同部分的系统通过发布和订阅事件来进行解耦通信。
// 当某个事件发生时，可以通过调用 RaiseEvent 方法来通知所有订阅了该事件的监听器。
public partial class RunEventHub : Node
{
	// Signal (信号)
	// 功能: C# 特性，用于标记一个委托为 Godot 信号。当信号被 EmitSignal 方法触发时，所有连接到该信号的监听器都会收到通知。
	[Signal]
	// Event (事件) Raised (触发) Event (事件) Handler (处理者)
	// 整体翻译: 事件触发事件处理委托
	// 功能: 定义了一个委托类型，用于处理 EventRaised 信号。这个委托有两个参数：
	// - eventId (事件ID)：一个字符串，唯一标识被触发的事件。
	// - scopeId (范围ID)：一个字符串，可选，用于进一步限定事件的范围或上下文。
	// 所有连接到 EventRaised 信号的方法都必须符合这个委托的签名。
	public delegate void EventRaisedEventHandler(string eventId, string scopeId);

	// Raise (触发) Event (事件)
	// 整体翻译: 触发事件
	// 功能: 用于触发 EventRaised 信号，向所有订阅者广播一个游戏事件。
	// eventId (事件ID)：要触发的事件的唯一标识符。
	// scopeId (范围ID)：可选，用于限定事件的范围，默认为空字符串。
	public void RaiseEvent(string eventId, string scopeId = "")
	{
		// string (字符串) Is (是) Null (空) Or (或者) White (空白) Space (空间)
		// 整体翻译: 字符串是否为空或空白
		// 功能: 检查 eventId 是否为 null、空字符串或只包含空白字符。如果是，则不触发事件，直接返回。
		if (string.IsNullOrWhiteSpace(eventId))
		{
			return;
		}

		// Emit (发出) Signal (信号)
		// 整体翻译: 发出信号
		// 功能: Godot方法，用于发出一个信号，通知所有连接到该信号的监听器。
		// Signal (信号) Name (名称) Event (事件) Raised (触发)
		// 整体翻译: 信号名称事件触发
		// 功能: Godot C# 的一个特性，提供类型安全的字符串常量，代表 EventRaised 信号的名称。
		EmitSignal(SignalName.EventRaised, eventId, scopeId ?? string.Empty);
	}
}
