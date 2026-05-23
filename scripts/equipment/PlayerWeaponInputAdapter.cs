using Godot;
using BattleHarvesterStudy.Presentation;

namespace BattleHarvesterStudy.Equipment;

public partial class PlayerWeaponInputAdapter : Node
{
	private const string WeaponSlot1Action = "weapon_slot_1";
	private const string WeaponSlot2Action = "weapon_slot_2";
	private const string WeaponSwapAction = "weapon_swap";

	private EquipmentComponent? _equipment;
	private GameplayInputGate? _gameplayInputGate;
	private Node3D? _owner;

	public override void _Ready()
	{
		_owner = GetOwner<Node3D>();
		_equipment = _owner?.GetNodeOrNull<EquipmentComponent>("Components/Equipment");
		_gameplayInputGate = _owner?.GetNodeOrNull<GameplayInputGate>("Components/GameplayInputGate");
		SetPhysicsProcess(true);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_equipment == null || (_gameplayInputGate?.BlocksCombatInput ?? false))
		{
			return;
		}

		if (Input.IsActionJustPressed(WeaponSlot1Action))
		{
			_equipment.TrySetActiveWeaponSlot(EquipmentSlotType.WeaponSlot1);
			return;
		}

		if (Input.IsActionJustPressed(WeaponSlot2Action))
		{
			_equipment.TrySetActiveWeaponSlot(EquipmentSlotType.WeaponSlot2);
			return;
		}

		if (Input.IsActionJustPressed(WeaponSwapAction))
		{
			_equipment.TryToggleActiveWeaponSlot();
		}
	}
}
