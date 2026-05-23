using Godot;
using BattleHarvesterStudy.Presentation;
using BattleHarvesterStudy.Equipment;

namespace BattleHarvesterStudy.Combat;

public partial class PlayerSkillInputAdapter : Node
{
	private const string BasicAttackAction = "attack";
	private const string SkillUAction = "skill_u";
	private const string SkillIAction = "skill_i";
	private const string SkillOAction = "skill_o";
	private const string SkillPAction = "skill_p";

	private ActorSkillLoadout? _skillLoadout;
	private ActorSkillCooldownController? _skillCooldowns;
	private CombatAimController? _aimController;
	private SkillChainTracker? _chainTracker;
	private StateMachine? _stateMachine;
	private EquipmentComponent? _equipment;
	private Node3D? _owner;
	private GameplayInputGate? _gameplayInputGate;

	public string LastAttemptedSkillName { get; private set; } = string.Empty;
	public SkillCastCheckResult LastCastCheckResult { get; private set; } = new()
	{
		CanCast = true,
		BlockReason = SkillCastBlockReason.None,
		FailureDetail = string.Empty
	};

	public override void _Ready()
	{
		_owner = GetOwner<Node3D>();
		_skillLoadout = _owner?.GetNodeOrNull<ActorSkillLoadout>("Components/SkillLoadout");
		_skillCooldowns = _owner?.GetNodeOrNull<ActorSkillCooldownController>("Components/SkillCooldowns");
		_aimController = _owner?.GetNodeOrNull<CombatAimController>("Components/CombatAimController");
		_chainTracker = _owner?.GetNodeOrNull<SkillChainTracker>("Components/SkillChainTracker");
		_stateMachine = _owner?.GetNodeOrNull<StateMachine>("StateMachine");
		_equipment = _owner?.GetNodeOrNull<EquipmentComponent>("Components/Equipment");
		_gameplayInputGate = _owner?.GetNodeOrNull<GameplayInputGate>("Components/GameplayInputGate");
		SetPhysicsProcess(true);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_skillLoadout == null || _skillCooldowns == null)
		{
			return;
		}

		if (_gameplayInputGate?.BlocksCombatInput ?? false)
		{
			return;
		}

		if (Input.IsActionJustPressed(BasicAttackAction) && _equipment?.GetActiveFirearmDefinition() == null)
		{
			TryQueueSkill(ResolveBasicAttackSkill());
			return;
		}

		if (Input.IsActionJustPressed(SkillUAction))
		{
			TryQueueSkill(_skillLoadout.GetSkillSlot1Definition());
			return;
		}

		if (Input.IsActionJustPressed(SkillIAction))
		{
			TryQueueSkill(_skillLoadout.GetSkillSlot2Definition());
			return;
		}

		if (Input.IsActionJustPressed(SkillOAction))
		{
			TryQueueSkill(_skillLoadout.GetSkillSlot3Definition());
			return;
		}

		if (Input.IsActionJustPressed(SkillPAction))
		{
			TryQueueSkill(_skillLoadout.GetSkillSlot4Definition());
		}
	}

	private void TryQueueSkill(SkillDefinition? skill)
	{
		if (_skillLoadout == null || _skillCooldowns == null || _owner == null || skill == null)
		{
			return;
		}

		LastAttemptedSkillName = skill.DisplayName;
		bool queued = QueuedSkillCastService.TryQueueSkill(_owner, skill, _skillLoadout, _skillCooldowns, _aimController, _chainTracker, out SkillCastCheckResult castCheckResult);
		LastCastCheckResult = castCheckResult;
		if (queued && _stateMachine?.CurrentState?.Name != "Attack")
		{
			_stateMachine.ChangeState("Attack");
		}
	}

	private SkillDefinition ResolveBasicAttackSkill()
	{
		if (_skillLoadout == null || _skillCooldowns == null || _owner == null)
		{
			return _skillLoadout?.GetBasicSkillDefinition() ?? throw new System.InvalidOperationException("Missing skill loadout for basic attack resolution.");
		}

		SkillDefinition finisher = _skillLoadout.GetBasicAttackFinisherSkillDefinition();
		if (MeetsRequirements(finisher))
		{
			return finisher;
		}

		SkillDefinition followup = _skillLoadout.GetBasicAttackFollowupSkillDefinition();
		if (MeetsRequirements(followup))
		{
			return followup;
		}

		return _skillLoadout.GetBasicSkillDefinition();
	}

	private bool MeetsRequirements(SkillDefinition skill)
	{
		if (_owner == null || _skillCooldowns == null)
		{
			return false;
		}

		SkillCastContext context = new(_owner, skill, _skillCooldowns, null, _aimController, _chainTracker);
		foreach (SkillCastRequirement requirement in skill.CastRequirements)
		{
			if (!requirement.Evaluate(context).Satisfied)
			{
				return false;
			}
		}

		return true;
	}
}
