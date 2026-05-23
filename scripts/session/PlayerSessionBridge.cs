using Godot;

namespace BattleHarvesterStudy.Session;

// Player (玩家) Session (会话) Bridge (桥梁)
// 整体翻译: 玩家会话桥接器
// 功能: 这是一个简单的脚本，用于将当前场景中的玩家节点与 RunSession 单例连接起来。
// 它确保 RunSession 知道哪个是当前的玩家，以便 RunSession 可以管理玩家的状态，
// 例如在场景切换时保存和加载玩家数据。
public partial class PlayerSessionBridge : Node
{
	// _Ready (就绪)
	// 功能: Godot生命周期方法，当节点及其子节点被添加到场景树并准备好时调用。
	// 在这里，它使用 CallDeferred 延迟调用 BindCurrentPlayer 方法，
	// 以确保在所有节点都准备好之后再进行玩家绑定。
	public override void _Ready()
	{
		CallDeferred(MethodName.BindCurrentPlayer);
	}

	// Bind (绑定) Current (当前) Player (玩家)
	// 整体翻译: 绑定当前玩家
	// 功能: 这是一个私有方法，用于获取此脚本所在节点的拥有者（通常是玩家节点），
	// 并将其传递给 RunSession 的 BindPlayer 方法。这使得 RunSession 能够跟踪和管理当前玩家。
	private void BindCurrentPlayer()
	{
		RunSession.Instance?.BindPlayer(GetOwnerOrNull<Node3D>());
	}
}
