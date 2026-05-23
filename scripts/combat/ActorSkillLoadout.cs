using Godot;
using System;

namespace BattleHarvesterStudy.Combat;

public partial class ActorSkillLoadout : Node
{
	private const string BasicSkillPath = "res://resources/combat/skills/basic_attack.tres";
	private const string BasicFollowupSkillPath = "res://resources/combat/skills/basic_attack_followup.tres";
	private const string BasicFinisherSkillPath = "res://resources/combat/skills/basic_attack_finisher.tres";

	[Export]
	public SkillDefinition? BasicSkillDefinition { get; set; }

	[Export]
	public SkillDefinition? BasicAttackFollowupSkillDefinition { get; set; }

	[Export]
	public SkillDefinition? BasicAttackFinisherSkillDefinition { get; set; }

	private SkillDefinition? _queuedSkill;
	private WeaponMoveSetDefinition? _weaponMoveSet;
	private EquipmentComponent? _equipment;
	public bool HasQueuedSkill => _queuedSkill != null;

	public event Action? SkillLoadoutChanged;

	public override void _Ready()
	{
		Node3D? owner = GetOwner<Node3D>();
		_equipment = owner?.GetNodeOrNull<EquipmentComponent>("Components/Equipment");
	}

	public SkillDefinition GetBasicSkillDefinition()
	{
		if (_weaponMoveSet?.PrimaryAttack != null)
		{
			return _weaponMoveSet.PrimaryAttack;
		}

		BasicSkillDefinition ??= LoadRequiredSkill(BasicSkillPath);
		return BasicSkillDefinition;
	}

	public SkillDefinition? GetSkillSlot1Definition()
	{
		return _equipment?.GetActiveWeaponSkillDefinition(0);
	}

	public SkillDefinition GetBasicAttackFollowupSkillDefinition()
	{
		if (_weaponMoveSet?.FollowupAttack != null)
		{
			return _weaponMoveSet.FollowupAttack;
		}

		BasicAttackFollowupSkillDefinition ??= LoadRequiredSkill(BasicFollowupSkillPath);
		return BasicAttackFollowupSkillDefinition;
	}

	public SkillDefinition GetBasicAttackFinisherSkillDefinition()
	{
		if (_weaponMoveSet?.FinisherAttack != null)
		{
			return _weaponMoveSet.FinisherAttack;
		}

		BasicAttackFinisherSkillDefinition ??= LoadRequiredSkill(BasicFinisherSkillPath);
		return BasicAttackFinisherSkillDefinition;
	}

	public SkillDefinition? GetSkillSlot2Definition()
	{
		return _equipment?.GetActiveWeaponSkillDefinition(1);
	}

	public SkillDefinition? GetSkillSlot3Definition()
	{
		return _equipment?.GetActiveWeaponSkillDefinition(2);
	}

	public SkillDefinition? GetSkillSlot4Definition()
	{
		return _equipment?.GetActiveWeaponSkillDefinition(3);
	}

	public void ApplyWeaponMoveSet(WeaponMoveSetDefinition? moveSet)
	{
		if (_weaponMoveSet == moveSet)
		{
			return;
		}

		_weaponMoveSet = moveSet;
		ClearQueuedSkill();
		SkillLoadoutChanged?.Invoke();
	}

	public void QueueSkill(SkillDefinition skill)
	{
		_queuedSkill = skill;
	}

	public SkillDefinition ConsumeQueuedSkillOrDefault()
	{
		SkillDefinition resolved = _queuedSkill ?? GetBasicSkillDefinition();
		_queuedSkill = null;
		return resolved;
	}

	public void ClearQueuedSkill()
	{
		_queuedSkill = null;
	}

	private static SkillDefinition LoadRequiredSkill(string path)
	{
		Resource? resource = GD.Load<Resource>(path);
		if (resource is SkillDefinition skill)
		{
			return skill;
		}

		throw new InvalidOperationException($"Missing required skill resource: {path}");
	}
}
