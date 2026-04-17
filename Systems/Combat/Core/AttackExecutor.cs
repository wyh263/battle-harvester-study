using Godot;
using System.Threading.Tasks;

namespace BattleHarvesterStudy;

public partial class AttackExecutor : Node
{
	[Signal]
	public delegate void AttackCompletedEventHandler();

	private Hitbox? _hitbox;
	private Label3D? _directionLabel;
	private Node3D? _owner;
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
		_directionLabel = _owner.GetNodeOrNull<Label3D>("Visuals/DirectionLabel");
	}

	public void Play(SkillDefinition skill)
	{
		_sequenceId++;
		_ = RunSkillAsync(_sequenceId, skill);
	}

	public void Cancel()
	{
		_sequenceId++;
		IsPlaying = false;
		if (_hitbox != null && CurrentContext != null)
		{
			CurrentSkill?.AttackBehavior?.End(_hitbox, CurrentContext);
		}

		CurrentSkill = null;
		CurrentContext = null;
	}

	private async Task RunSkillAsync(int sequenceId, SkillDefinition skill)
	{
		if (_owner == null)
		{
			return;
		}

		Vector3 facingDirection = -_owner.GlobalTransform.Basis.Z;
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

		SetLabel(skill.StartupLabel);
		await WaitSeconds(skill.StartupSeconds);
		if (!IsCurrentSequence(sequenceId))
		{
			return;
		}

		SetLabel(skill.ActiveLabel);
		skill.AttackBehavior?.Begin(_hitbox, CurrentContext);
		await WaitSeconds(skill.ActiveSeconds);
		if (!IsCurrentSequence(sequenceId))
		{
			return;
		}

		skill.AttackBehavior?.End(_hitbox, CurrentContext);
		SetLabel(skill.RecoveryLabel);
		await WaitSeconds(skill.RecoverySeconds);
		if (!IsCurrentSequence(sequenceId))
		{
			return;
		}

		IsPlaying = false;
		CurrentSkill = null;
		CurrentContext = null;
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

	private void SetLabel(string text)
	{
		if (_directionLabel == null)
		{
			return;
		}

		_directionLabel.Text = text;
	}
}
