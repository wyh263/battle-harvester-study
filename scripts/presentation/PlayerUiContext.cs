using Godot;
using BattleHarvesterStudy.Inventory.Search;
using BattleHarvesterStudy.Combat.Firearms;

namespace BattleHarvesterStudy.Presentation;

public partial class PlayerUiContext : Node
{
	public ActorSkillLoadout? SkillLoadout { get; private set; }
	public ActorSkillCooldownController? SkillCooldowns { get; private set; }
	public ActorSkillResourceController? SkillResources { get; private set; }
	public CombatAimController? AimController { get; private set; }
	public SkillChainTracker? SkillChainTracker { get; private set; }
	public HealthComponent? PlayerHealth { get; private set; }
	public ArmorComponent? PlayerArmor { get; private set; }
	public InventoryComponent? Inventory { get; private set; }
	public EquipmentComponent? Equipment { get; private set; }
	public GridContainerComponent? SecureContainer { get; private set; }
	public DefaultContainerModifierComponent? ContainerModifiers { get; private set; }
	public ActiveContainerModifierComponent? ActiveContainerModifiers { get; private set; }
	public FirearmCombatComponent? FirearmCombat { get; private set; }

	public override void _Ready()
	{
		Node3D? gameplayRoot = UiNodeLocator.ResolveGameplayRoot(this);
		SkillLoadout = gameplayRoot?.GetNodeOrNull<ActorSkillLoadout>("Components/SkillLoadout");
		SkillCooldowns = gameplayRoot?.GetNodeOrNull<ActorSkillCooldownController>("Components/SkillCooldowns");
		SkillResources = gameplayRoot?.GetNodeOrNull<ActorSkillResourceController>("Components/SkillResources");
		AimController = gameplayRoot?.GetNodeOrNull<CombatAimController>("Components/CombatAimController");
		SkillChainTracker = gameplayRoot?.GetNodeOrNull<SkillChainTracker>("Components/SkillChainTracker");
		PlayerHealth = gameplayRoot?.GetNodeOrNull<HealthComponent>("Components/Health");
		PlayerArmor = gameplayRoot?.GetNodeOrNull<ArmorComponent>("Components/Armor");
		Inventory = gameplayRoot?.GetNodeOrNull<InventoryComponent>("Components/Inventory");
		Equipment = gameplayRoot?.GetNodeOrNull<EquipmentComponent>("Components/Equipment");
		SecureContainer = gameplayRoot?.GetNodeOrNull<GridContainerComponent>("Components/SecureContainer");
		ContainerModifiers = gameplayRoot?.GetNodeOrNull<DefaultContainerModifierComponent>("Components/ContainerModifiers");
		ActiveContainerModifiers = gameplayRoot?.GetNodeOrNull<ActiveContainerModifierComponent>("Components/ActiveContainerModifiers");
		FirearmCombat = gameplayRoot?.GetNodeOrNull<FirearmCombatComponent>("Components/FirearmCombat");
	}

	public HealthComponent? ResolveHealth(Node3D? actor)
	{
		return actor?.GetNodeOrNull<HealthComponent>("Components/Health");
	}

	public ArmorComponent? ResolveArmor(Node3D? actor = null)
	{
		if (actor == null)
		{
			Node3D? gameplayRoot = UiNodeLocator.ResolveGameplayRoot(this);
			return gameplayRoot?.GetNodeOrNull<ArmorComponent>("Components/Armor");
		}

		return actor.GetNodeOrNull<ArmorComponent>("Components/Armor");
	}

	public StatsComponent? ResolveStats(Node3D? actor = null)
	{
		if (actor == null)
		{
			Node3D? gameplayRoot = UiNodeLocator.ResolveGameplayRoot(this);
			return gameplayRoot?.GetNodeOrNull<StatsComponent>("Components/Stats");
		}

		return actor.GetNodeOrNull<StatsComponent>("Components/Stats");
	}
}
