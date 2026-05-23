using Godot;
using BattleHarvesterStudy.Attributes;
using BattleHarvesterStudy.Combat;
using BattleHarvesterStudy.Equipment;
using BattleHarvesterStudy.Presentation;

namespace BattleHarvesterStudy.Items;

public partial class PlayerItemInputAdapter : Node
{
	private const string Item1Action = "item_slot_1";
	private const string Item2Action = "item_slot_2";
	private const string Item3Action = "item_slot_3";

	private EquipmentComponent? _equipment;
	private HealthComponent? _health;
	private ActorSkillResourceController? _resources;
	private ActorSkillLoadout? _skillLoadout;
	private ActorSkillCooldownController? _skillCooldowns;
	private CombatAimController? _aimController;
	private SkillChainTracker? _chainTracker;
	private Node3D? _owner;
	private GameplayInputGate? _gameplayInputGate;

	public override void _Ready()
	{
		_owner = GetOwner<Node3D>();
		_equipment = _owner?.GetNodeOrNull<EquipmentComponent>("Components/Equipment");
		_health = _owner?.GetNodeOrNull<HealthComponent>("Components/Health");
		_resources = _owner?.GetNodeOrNull<ActorSkillResourceController>("Components/SkillResources");
		_skillLoadout = _owner?.GetNodeOrNull<ActorSkillLoadout>("Components/SkillLoadout");
		_skillCooldowns = _owner?.GetNodeOrNull<ActorSkillCooldownController>("Components/SkillCooldowns");
		_aimController = _owner?.GetNodeOrNull<CombatAimController>("Components/CombatAimController");
		_chainTracker = _owner?.GetNodeOrNull<SkillChainTracker>("Components/SkillChainTracker");
		_gameplayInputGate = _owner?.GetNodeOrNull<GameplayInputGate>("Components/GameplayInputGate");
		SetPhysicsProcess(true);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_equipment == null || (_gameplayInputGate?.IsBlocked ?? false))
		{
			return;
		}

		if (Input.IsActionJustPressed(Item1Action))
		{
			TryUseSlot(EquipmentSlotType.Item1);
			return;
		}

		if (Input.IsActionJustPressed(Item2Action))
		{
			TryUseSlot(EquipmentSlotType.Item2);
			return;
		}

		if (Input.IsActionJustPressed(Item3Action))
		{
			TryUseSlot(EquipmentSlotType.Item3);
		}
	}

	private void TryUseSlot(EquipmentSlotType slotType)
	{
		if (_equipment == null)
		{
			return;
		}

		if (!EquippedItemUseService.TryUseEquippedItem(_equipment, slotType, _health, _resources, _owner, _skillLoadout, _skillCooldowns, _aimController, _chainTracker, out ItemInstance? item, out ItemUseFailureReason failureReason))
		{
			GD.Print($"[ItemUse] {slotType}: failed ({failureReason})");
			return;
		}

		if (item != null)
		{
			GD.Print($"[ItemUse] {slotType}: used {item.Definition.DisplayName}");
		}
	}
}
