using Godot;

namespace BattleHarvesterStudy.Presentation;

public partial class SkillPhaseLabelPresenter : Node
{
	private Node3D? _owner;
	private Label3D? _label;

	public override void _Ready()
	{
		_owner = GetOwner<Node3D>();
		_label = _owner?.GetNodeOrNull<Label3D>("Visuals/DirectionLabel");
		CombatPresentationEvents.SkillPhaseChanged += OnSkillPhaseChanged;
	}

	public override void _ExitTree()
	{
		CombatPresentationEvents.SkillPhaseChanged -= OnSkillPhaseChanged;
	}

	private void OnSkillPhaseChanged(SkillPresentationEvent skillEvent)
	{
		if (_owner == null || _label == null)
		{
			return;
		}

		if (skillEvent.Context.Caster != _owner)
		{
			return;
		}

		_label.Text = ResolvePhaseLabel(skillEvent);
	}

	private static string ResolvePhaseLabel(SkillPresentationEvent skillEvent)
	{
		return skillEvent.Phase switch
		{
			SkillPresentationPhase.Startup => skillEvent.Context.Skill.StartupLabel,
			SkillPresentationPhase.Active => skillEvent.Context.Skill.ActiveLabel,
			SkillPresentationPhase.Recovery => skillEvent.Context.Skill.RecoveryLabel,
			_ => ResolveIdleLabel(skillEvent.Context.Caster)
		};
	}

	private static string ResolveIdleLabel(Node3D caster)
	{
		if (caster is IStateActor actor)
		{
			return actor.FacingLabel;
		}

		return string.Empty;
	}
}
