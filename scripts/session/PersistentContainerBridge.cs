using Godot;

namespace BattleHarvesterStudy.Session;

// Persistent (持久的) Container (容器) Bridge (桥梁)
// 整体翻译: 持久容器桥接器
// 功能: 该脚本作为 Godot 场景中的容器（如仓库或安全箱）与全局会话管理（RunSession）之间的桥梁。
// 它负责在场景加载时从 RunSession 应用容器的持久化状态，并在容器内容发生变化时将新状态持久化回 RunSession。
// 这确保了在不同场景或游戏会话之间，容器中的物品状态能够被正确保存和恢复。
public partial class PersistentContainerBridge : Node
{
	// Persistent (持久的) Container (容器) Kind (种类)
	// 整体翻译: 持久容器类型
	// 功能: 定义了两种不同类型的持久化容器。
	public enum PersistentContainerKind
	{
		// Warehouse (仓库)
		// 功能: 表示游戏中的仓库容器。
		Warehouse = 0,
		// Secure (安全的) Container (容器)
		// 功能: 表示游戏中的安全箱容器，可能用于存储特殊物品。
		SecureContainer = 1,
	}

	// Container (容器) Kind (种类)
	// 功能: 导出变量，用于在 Godot 编辑器中选择此桥接器管理的容器类型（仓库或安全箱）。
	[Export]
	public PersistentContainerKind ContainerKind { get; set; } = PersistentContainerKind.Warehouse;

	// Container (容器) Path (路径)
	// 功能: 导出变量，指定场景中实际 GridContainerComponent 节点的路径。
	[Export]
	public NodePath ContainerPath { get; set; } = new("../Components/GridContainer");

	// _container (容器)
	// 功能: 私有字段，用于存储获取到的 GridContainerComponent 实例的引用。
	private GridContainerComponent? _container;

		// _Ready (就绪)
	// 功能: Godot生命周期方法，当节点及其子节点被添加到场景树并准备好时调用。
	// 在此方法中，它首先获取 GridContainerComponent 的引用，然后调用 ApplySnapshot 来应用持久化状态。
	// 如果容器成功获取，它会订阅容器的 ContainerChanged 事件，以便在容器内容变化时进行持久化。
	public override void _Ready()
	{
		_container = GetNodeOrNull<GridContainerComponent>(ContainerPath);
		if (ContainerKind == PersistentContainerKind.SecureContainer)
		{
			RunSession.Instance?.RegisterSecureContainer(_container?.Definition);
		}
		ApplySnapshot();
		if (_container != null)
		{
			_container.ContainerChanged += OnContainerChanged;
		}
	}

	// _Exit (退出) Tree (树)
	// 功能: Godot生命周期方法，当节点从场景树中移除时调用。
	// 在此方法中，它负责取消订阅 ContainerChanged 事件，以防止在节点被销毁后仍尝试访问已释放的容器，避免潜在的内存泄漏或空引用错误。
	public override void _ExitTree()
	{
		if (_container != null)
		{
			_container.ContainerChanged -= OnContainerChanged;
		}
	}

		// Apply (应用) Snapshot (快照)
	// 整体翻译: 应用快照
	// 功能: 这是一个私有辅助方法，用于根据 ContainerKind 从 RunSession 获取并应用对应的持久化容器状态。
	// 它首先尝试获取 RunSession 的单例实例。如果成功获取，则根据 ContainerKind 的值（Warehouse 或 SecureContainer），
	// 调用 RunSession 相应的应用状态方法（ApplyWarehouseState 或 ApplySecureContainerState），
	// 将之前保存的物品状态加载到当前的 _container 中。
	private void ApplySnapshot()
	{
		RunSession? runSession = RunSession.Instance;
		if (runSession == null)
		{
			return;
		}

		if (ContainerKind == PersistentContainerKind.Warehouse)
		{
			runSession.ApplyWarehouseState(_container);
			return;
		}

		runSession.ApplySecureContainerState(_container);
	}

		// On (当) Container (容器) Changed (改变)
	// 整体翻译: 当容器改变时
	// 功能: 这是一个事件处理方法，当绑定的容器 (_container) 的内容发生变化时会被调用。
	// 它首先尝试获取 RunSession 的单例实例。如果成功获取，则根据 ContainerKind 的值（Warehouse 或 SecureContainer），
	// 调用 RunSession 相应的持久化状态方法（PersistWarehouseState 或 PersistSecureContainerState），
	// 将当前 _container 的状态保存到 RunSession 中，以实现持久化。
	private void OnContainerChanged()
	{
		RunSession? runSession = RunSession.Instance;
		if (runSession == null)
		{
			return;
		}

		if (ContainerKind == PersistentContainerKind.Warehouse)
		{
			runSession.PersistWarehouseState(_container);
			return;
		}

		runSession.PersistSecureContainerState(_container);
	}
}
