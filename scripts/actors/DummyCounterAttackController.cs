using Godot;
using System.Threading.Tasks;
using BattleHarvesterStudy.Equipment;

namespace BattleHarvesterStudy.Actors;

public partial class DummyCounterAttackController : Node
{
	[Export]
	public bool CounterEnabled { get; set; } = false;

	[Export(PropertyHint.Range, "0,5,0.01")]
	public float CounterDelaySeconds { get; set; } = 1.0f;

	private Dummy? _owner;
	private Hurtbox? _hurtbox;
	private CombatAimController? _aimController;
	private ActorSkillLoadout? _skillLoadout;
	private ActorSkillCooldownController? _skillCooldowns;
	private ActorSkillResourceController? _skillResources;
	private SkillChainTracker? _chainTracker;
	private EquipmentComponent? _equipment;
	private int _counterSequenceId;

	public override void _Ready()
	{
		_owner = GetOwner<Dummy>();
		_hurtbox = _owner?.GetNodeOrNull<Hurtbox>("Hurtbox");
		_aimController = _owner?.GetNodeOrNull<CombatAimController>("Components/CombatAimController");
		_skillLoadout = _owner?.GetNodeOrNull<ActorSkillLoadout>("Components/SkillLoadout");
		_skillCooldowns = _owner?.GetNodeOrNull<ActorSkillCooldownController>("Components/SkillCooldowns");
		_skillResources = _owner?.GetNodeOrNull<ActorSkillResourceController>("Components/SkillResources");
		_chainTracker = _owner?.GetNodeOrNull<SkillChainTracker>("Components/SkillChainTracker");
		_equipment = _owner?.GetNodeOrNull<EquipmentComponent>("Components/Equipment");

		if (_hurtbox != null)
		{
			_hurtbox.DamageTaken += OnDamageTaken;
		}
	}

	public override void _ExitTree()
	{
		if (_hurtbox != null)
		{
			_hurtbox.DamageTaken -= OnDamageTaken;
		}
	}

	private void OnDamageTaken(DamageInfo info)
	{
		if (!CounterEnabled)
		{
			return;
		}

		_counterSequenceId++;
		_ = QueueCounterAsync(_counterSequenceId, info.Attacker);
	}

	private async Task QueueCounterAsync(int sequenceId, Node3D? attacker)
	{
		await ToSignal(GetTree().CreateTimer(CounterDelaySeconds), SceneTreeTimer.SignalName.Timeout);

		if (!IsInsideTree() || sequenceId != _counterSequenceId || _owner == null
			|| _skillLoadout == null || _skillCooldowns == null)
		{
			return;
		}

		if (attacker != null)
		{
			Targetable? targetable = attacker.GetNodeOrNull<Targetable>("Components/Targetable");
			if (targetable != null)
			{
				_aimController?.SetLockedTarget(targetable);
			}
		}

		SkillDefinition skill = _skillLoadout.GetBasicSkillDefinition();
		if (!skill.SupportsWeapon(_equipment?.GetActiveWeaponDefinition()))
		{
			return;
		}

		SkillCastCheckResult castCheck = _skillCooldowns.CheckCast(skill);
		if (!castCheck.CanCast)
		{
			return;
		}

		SkillCastContext context = new(_owner, skill, _skillCooldowns, _skillResources, _aimController, _chainTracker);
		foreach (SkillCastRequirement requirement in skill.CastRequirements)
		{
			if (!requirement.Evaluate(context).Satisfied)
			{
				return;
			}
		}

		_skillLoadout.QueueSkill(skill);
	}
}
