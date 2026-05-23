using Godot;
using System.Collections.Generic;
using BattleHarvesterStudy.Combat.Firearms;

namespace BattleHarvesterStudy.Presentation;

public partial class CombatHudPresenter : Node
{
	private sealed record SkillSlot(string Name, System.Func<SkillDefinition?> ResolveSkill, Label Label);

	private ActorSkillLoadout? _skillLoadout;
	private ActorSkillCooldownController? _skillCooldowns;
	private ActorSkillResourceController? _skillResources;
	private CombatAimController? _aimController;
	private HealthComponent? _playerHealth;
	private ArmorComponent? _playerArmor;
	private FirearmCombatComponent? _firearmCombat;
	private PlayerUiContext? _uiContext;
	private Label? _resourceLabel;
	private ProgressBar? _resourceBar;
	private Label? _playerHealthLabel;
	private ProgressBar? _playerHealthBar;
	private Label? _playerArmorLabel;
	private ProgressBar? _playerArmorBar;
	private Label? _targetLabel;
	private Label? _targetHealthLabel;
	private ProgressBar? _targetHealthBar;
	private Label? _targetArmorLabel;
	private ProgressBar? _targetArmorBar;
	private readonly List<SkillSlot> _skillSlots = [];

	public override void _Ready()
	{
		Node3D? owner = UiNodeLocator.ResolveGameplayRoot(this);
		_uiContext = owner?.GetNodeOrNull<PlayerUiContext>("GameUiRoot/Controllers/PlayerUiContext");
		_skillLoadout = _uiContext?.SkillLoadout;
		_skillCooldowns = _uiContext?.SkillCooldowns;
		_skillResources = _uiContext?.SkillResources;
		_aimController = _uiContext?.AimController;
		_playerHealth = _uiContext?.PlayerHealth;
		_playerArmor = _uiContext?.PlayerArmor;
		_firearmCombat = _uiContext?.FirearmCombat;

		_playerHealthLabel = owner?.GetNodeOrNull<Label>("GameUiRoot/TopLeft/TopPanel/Margin/VBox/HealthLabel");
		_playerHealthBar = owner?.GetNodeOrNull<ProgressBar>("GameUiRoot/TopLeft/TopPanel/Margin/VBox/HealthBar");
		_playerArmorLabel = owner?.GetNodeOrNull<Label>("GameUiRoot/TopLeft/TopPanel/Margin/VBox/ArmorLabel");
		_playerArmorBar = owner?.GetNodeOrNull<ProgressBar>("GameUiRoot/TopLeft/TopPanel/Margin/VBox/ArmorBar");
		_resourceLabel = owner?.GetNodeOrNull<Label>("GameUiRoot/TopLeft/TopPanel/Margin/VBox/ResourceLabel");
		_resourceBar = owner?.GetNodeOrNull<ProgressBar>("GameUiRoot/TopLeft/TopPanel/Margin/VBox/ResourceBar");
		_targetLabel = owner?.GetNodeOrNull<Label>("GameUiRoot/TopLeft/TopPanel/Margin/VBox/TargetLabel");
		_targetHealthLabel = owner?.GetNodeOrNull<Label>("GameUiRoot/TargetTop/TargetPanel/Margin/VBox/TargetHealthLabel");
		_targetHealthBar = owner?.GetNodeOrNull<ProgressBar>("GameUiRoot/TargetTop/TargetPanel/Margin/VBox/TargetHealthBar");
		_targetArmorLabel = owner?.GetNodeOrNull<Label>("GameUiRoot/TargetTop/TargetPanel/Margin/VBox/TargetArmorLabel");
		_targetArmorBar = owner?.GetNodeOrNull<ProgressBar>("GameUiRoot/TargetTop/TargetPanel/Margin/VBox/TargetArmorBar");

		if (_skillLoadout != null)
		{
			AddSkillSlot(owner, "ATK", _skillLoadout.GetBasicSkillDefinition, "GameUiRoot/BottomBar/SkillPanel/Margin/Slots/AtkSlot");
			AddSkillSlot(owner, "U", _skillLoadout.GetSkillSlot1Definition, "GameUiRoot/BottomBar/SkillPanel/Margin/Slots/USlot");
			AddSkillSlot(owner, "I", _skillLoadout.GetSkillSlot2Definition, "GameUiRoot/BottomBar/SkillPanel/Margin/Slots/ISlot");
			AddSkillSlot(owner, "O", _skillLoadout.GetSkillSlot3Definition, "GameUiRoot/BottomBar/SkillPanel/Margin/Slots/OSlot");
			AddSkillSlot(owner, "P", _skillLoadout.GetSkillSlot4Definition, "GameUiRoot/BottomBar/SkillPanel/Margin/Slots/PSlot");
		}

		if (_aimController != null)
		{
			_aimController.LockedTargetStateChanged += OnLockedTargetChanged;
		}

		RefreshTargetLabel();
		SetProcess(true);
	}

	public override void _ExitTree()
	{
		if (_aimController != null)
		{
			_aimController.LockedTargetStateChanged -= OnLockedTargetChanged;
		}
	}

	public override void _Process(double delta)
	{
		RefreshPlayerHealth();
		RefreshPlayerArmor();
		RefreshResource();
		RefreshSkillSlots();
		RefreshTargetHealth();
		RefreshTargetArmor();
		RefreshTargetLabel();
	}

	private void AddSkillSlot(Node3D? owner, string name, System.Func<SkillDefinition?> resolveSkill, string path)
	{
		Label? label = owner?.GetNodeOrNull<Label>(path);
		if (label == null)
		{
			return;
		}

		_skillSlots.Add(new SkillSlot(name, resolveSkill, label));
	}

	private void RefreshResource()
	{
		if (_skillResources == null || _resourceBar == null || _resourceLabel == null)
		{
			return;
		}

		_resourceBar.MaxValue = _skillResources.MaxResource;
		_resourceBar.Value = _skillResources.CurrentResource;
		_resourceLabel.Text = UiText.Resolve(
			UiTextKeys.Hud.Resource,
			("label", _skillResources.ResourceLabel),
			("current", _skillResources.CurrentResource),
			("max", _skillResources.MaxResource));
	}

	private void RefreshPlayerHealth()
	{
		if (_playerHealth == null || _playerHealthBar == null || _playerHealthLabel == null)
		{
			return;
		}

		_playerHealthBar.MaxValue = _playerHealth.MaxHealth;
		_playerHealthBar.Value = _playerHealth.CurrentHealth;
		_playerHealthLabel.Text = _playerHealth.IsDead
			? UiText.Resolve(UiTextKeys.Hud.HealthDead)
			: UiText.Resolve(UiTextKeys.Hud.Health, ("current", _playerHealth.CurrentHealth), ("max", _playerHealth.MaxHealth));
	}

	private void RefreshPlayerArmor()
	{
		if (_playerArmor == null || _playerArmorBar == null || _playerArmorLabel == null)
		{
			return;
		}

		float maxArmor = Mathf.Max(1.0f, _playerArmor.MaxArmor);
		_playerArmorBar.MaxValue = maxArmor;
		_playerArmorBar.Value = _playerArmor.CurrentArmor;
		_playerArmorBar.Visible = true;
		_playerArmorLabel.Visible = true;
		_playerArmorLabel.Text = UiText.Resolve(UiTextKeys.Hud.Armor, ("current", _playerArmor.CurrentArmor), ("max", _playerArmor.MaxArmor));
	}

	private void RefreshSkillSlots()
	{
		if (_skillCooldowns == null)
		{
			return;
		}

		foreach (SkillSlot slot in _skillSlots)
		{
			SkillDefinition? skill = slot.ResolveSkill();
			if (skill == null)
			{
				slot.Label.Text = UiText.Resolve(
					UiTextKeys.Hud.SkillSlot,
					("slot", slot.Name),
					("skill", UiText.CurrentLocale == UiText.DefaultLocale ? "未装配" : "Empty"),
					("state", "--"));
				slot.Label.Modulate = new Color(0.62f, 0.60f, 0.55f);
				continue;
			}

			float remaining = _skillCooldowns.GetRemainingCooldown(skill);
			slot.Label.Text = UiText.Resolve(
				UiTextKeys.Hud.SkillSlot,
				("slot", slot.Name),
				("skill", skill.DisplayName),
				("state", FormatRemaining(remaining)));
			slot.Label.Modulate = remaining <= 0.0f
				? new Color(1.0f, 0.95f, 0.82f)
				: new Color(0.82f, 0.80f, 0.72f);
		}
	}

	private void OnLockedTargetChanged(Node3D? target, bool isLocked)
	{
		RefreshTargetLabel();
	}

	private void RefreshTargetLabel()
	{
		if (_targetLabel == null)
		{
			return;
		}

		Node3D? target = ResolveDisplayedTarget();
		if (target != null)
		{
			string targetText = UiText.Resolve(UiTextKeys.Hud.Target, ("name", target.Name));
			if (_firearmCombat?.CurrentTarget == target)
			{
				targetText += $"  HIT {_firearmCombat.CurrentHitChance:0.#}%";
			}

			_targetLabel.Text = targetText;
			return;
		}

		_targetLabel.Text = UiText.Resolve(UiTextKeys.Hud.TargetFree);
	}

	private void RefreshTargetHealth()
	{
		if (_targetHealthLabel == null || _targetHealthBar == null)
		{
			return;
		}

		Node3D? target = ResolveDisplayedTarget();
		if (target == null)
		{
			_targetHealthLabel.Text = UiText.Resolve(UiTextKeys.Hud.TargetNone);
			_targetHealthBar.MaxValue = 1.0;
			_targetHealthBar.Value = 0.0;
			return;
		}

		HealthComponent? targetHealth = _uiContext?.ResolveHealth(target);
		if (targetHealth == null)
		{
			_targetHealthLabel.Text = UiText.Resolve(UiTextKeys.Hud.Target, ("name", $"{target.Name}  --"));
			_targetHealthBar.MaxValue = 1.0;
			_targetHealthBar.Value = 0.0;
			return;
		}

		_targetHealthBar.MaxValue = targetHealth.MaxHealth;
		_targetHealthBar.Value = targetHealth.CurrentHealth;
		_targetHealthLabel.Text = targetHealth.IsDead
			? UiText.Resolve(UiTextKeys.Hud.TargetDead, ("name", target.Name))
			: UiText.Resolve(UiTextKeys.Hud.Target, ("name", $"{target.Name}  {targetHealth.CurrentHealth:0}/{targetHealth.MaxHealth:0}"));
	}

	private void RefreshTargetArmor()
	{
		if (_targetArmorLabel == null || _targetArmorBar == null)
		{
			return;
		}

		Node3D? target = ResolveDisplayedTarget();
		if (target == null)
		{
			_targetArmorLabel.Text = UiText.CurrentLocale == UiText.DefaultLocale ? "护甲  --" : "Armor  --";
			_targetArmorBar.MaxValue = 1.0;
			_targetArmorBar.Value = 0.0;
			return;
		}

		ArmorComponent? targetArmor = _uiContext?.ResolveArmor(target);
		if (targetArmor == null || targetArmor.MaxArmor <= 0.0f)
		{
			_targetArmorLabel.Text = UiText.Resolve(UiTextKeys.Hud.Armor, ("current", 0), ("max", 0));
			_targetArmorBar.MaxValue = 1.0;
			_targetArmorBar.Value = 0.0;
			return;
		}

		_targetArmorBar.MaxValue = Mathf.Max(1.0f, targetArmor.MaxArmor);
		_targetArmorBar.Value = targetArmor.CurrentArmor;
		_targetArmorLabel.Text = UiText.Resolve(UiTextKeys.Hud.Armor, ("current", targetArmor.CurrentArmor), ("max", targetArmor.MaxArmor));
	}

	private Node3D? ResolveDisplayedTarget()
	{
		if (_firearmCombat?.CurrentTarget != null)
		{
			return _firearmCombat.CurrentTarget;
		}

		if (_aimController != null && _aimController.TryGetLockedTarget(out Node3D target))
		{
			return target;
		}

		return null;
	}

	private static string FormatRemaining(float remaining)
	{
		return remaining <= 0.0f
			? UiText.Resolve(UiTextKeys.Hud.Ready)
			: UiText.Resolve(UiTextKeys.Hud.Seconds, ("seconds", remaining));
	}
}
