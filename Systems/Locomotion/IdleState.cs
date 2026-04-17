using Godot;

namespace BattleHarvesterStudy;

public partial class IdleState : State
{
	public override void Enter()
	{
		base.Enter();
		Entity.DesiredMoveDirection = Vector3.Zero;
		Entity.RemoveMoveSpeedModifier(Player.RunSpeedModifierKey);
		SetDirectionLabelToFront();
		GD.Print("Enter Idle");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionJustPressed("dash") && Entity.CanStartDash())
		{
			Machine.ChangeState("Dash");
			return;
		}

		if (Input.IsActionJustPressed("attack"))
		{
			Entity.QueueSkill(Entity.GetBasicSkillDefinition());
			Machine.ChangeState("Attack");
			return;
		}

		if (Input.IsActionJustPressed("skill_u"))
		{
			Entity.QueueSkill(Entity.GetLaserSkillDefinition());
			Machine.ChangeState("Attack");
			return;
		}

		if (Input.IsActionJustPressed("skill_i"))
		{
			Entity.QueueSkill(Entity.GetWideBleedSlashSkillDefinition());
			Machine.ChangeState("Attack");
			return;
		}

		if (Input.IsActionJustPressed("skill_o"))
		{
			Entity.QueueSkill(Entity.GetSlowOrbSkillDefinition());
			Machine.ChangeState("Attack");
			return;
		}

		Vector2 input = Input.GetVector("move_left", "move_right", "move_up", "move_down");

		if (input != Vector2.Zero)
		{
			Machine.ChangeState("Move");
		}
	}

	private void SetDirectionLabelToFront()
	{
		Label3D? label = Entity.GetNodeOrNull<Label3D>("Visuals/DirectionLabel");
		if (label == null)
		{
			return;
		}

		if (label.Text == "ATTACK")
		{
			label.Text = Entity.FacingLabel;
		}
	}
}
