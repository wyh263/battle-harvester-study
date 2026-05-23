using Godot;
using System.Threading.Tasks;

namespace BattleHarvesterStudy.Combat;

public partial class AttackExecutor : Node
{
	[Signal]
	public delegate void AttackCompletedEventHandler();

	private Hitbox? _hitbox;
	private Node3D? _owner;
	private CombatAimController? _aimController;
	private IAimFacingHost? _facingHost;
	private SkillChainTracker? _chainTracker;
	private int _sequenceId;

	public bool IsPlaying { get; private set; }
	public SkillDefinition? CurrentSkill { get; private set; }
	public SkillExecutionContext? CurrentContext { get; private set; }

	public override void _Ready()
	{
		_owner = GetOwner<Node3D>();
		if (_owner == null)
		{
			return;
		}

		_hitbox = _owner.GetNodeOrNull<Hitbox>("Combat/Hitbox");
		_aimController = _owner.GetNodeOrNull<CombatAimController>("Components/CombatAimController");
		_facingHost = _owner as IAimFacingHost;
		_chainTracker = _owner.GetNodeOrNull<SkillChainTracker>("Components/SkillChainTracker");
	}

	public void Play(SkillDefinition skill)
	{
		_sequenceId++;
		PublishCurrentSkillPhase(SkillPresentationPhase.Cancelled);
		ClearCurrentSkill();
		_ = RunSkillAsync(_sequenceId, skill);
	}

	public void Cancel()
	{
		_sequenceId++;
		IsPlaying = false;
		PublishCurrentSkillPhase(SkillPresentationPhase.Cancelled);
		ClearCurrentSkill();
	}

	private async Task RunSkillAsync(int sequenceId, SkillDefinition skill)
	{
		if (_owner == null)
		{
			return;
		}

		Vector3 facingDirection;
		if (_aimController != null && _aimController.TryGetAimDirection(out Vector3 aimDirection))
		{
			facingDirection = aimDirection;
			_facingHost?.FaceWorldDirection(facingDirection);
		}
		else
		{
			facingDirection = -_owner.GlobalTransform.Basis.Z;
		}

		facingDirection.Y = 0.0f;
		if (facingDirection == Vector3.Zero)
		{
			facingDirection = Vector3.Forward;
		}

		CurrentSkill = skill;
		CurrentContext = new SkillExecutionContext(
			_owner,
			skill,
			_owner.GlobalPosition,
			facingDirection.Normalized()
		);
		IsPlaying = true;
		_chainTracker?.BeginSkill(skill);

		PublishSkillPhase(SkillPresentationPhase.Startup);
		await WaitSeconds(skill.StartupSeconds);
		if (!IsCurrentSequence(sequenceId))
		{
			return;
		}

		PublishSkillPhase(SkillPresentationPhase.Active);
		skill.AttackBehavior?.Begin(_hitbox, CurrentContext);
		await WaitSeconds(skill.ActiveSeconds);
		if (!IsCurrentSequence(sequenceId))
		{
			return;
		}

		skill.AttackBehavior?.End(_hitbox, CurrentContext);
		PublishSkillPhase(SkillPresentationPhase.Recovery);
		await WaitSeconds(skill.RecoverySeconds);
		if (!IsCurrentSequence(sequenceId))
		{
			return;
		}

		IsPlaying = false;
		PublishSkillPhase(SkillPresentationPhase.Completed);
		ClearCurrentSkill();
		EmitSignal(SignalName.AttackCompleted);
	}

	private async Task WaitSeconds(double seconds)
	{
		await ToSignal(GetTree().CreateTimer(seconds), SceneTreeTimer.SignalName.Timeout);
	}

	private bool IsCurrentSequence(int sequenceId)
	{
		if (!IsInsideTree() || _sequenceId != sequenceId)
		{
			return false;
		}

		if (!IsPlaying || CurrentSkill == null || CurrentContext == null)
		{
			return false;
		}

		return true;
	}

	private void ClearCurrentSkill()
	{
		if (_hitbox != null && CurrentContext != null)
		{
			CurrentSkill?.AttackBehavior?.End(_hitbox, CurrentContext);
		}

		CurrentSkill = null;
		CurrentContext = null;
	}

	private void PublishCurrentSkillPhase(SkillPresentationPhase phase)
	{
		if (CurrentContext == null)
		{
			return;
		}

		_chainTracker?.UpdatePhase(phase);
		CombatPresentationEvents.PublishSkillPhase(CurrentContext, phase);
	}

	private void PublishSkillPhase(SkillPresentationPhase phase)
	{
		PublishCurrentSkillPhase(phase);
	}
}
