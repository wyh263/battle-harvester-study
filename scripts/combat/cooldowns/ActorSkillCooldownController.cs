using Godot;
using System;
using System.Collections.Generic;

namespace BattleHarvesterStudy.Combat;

public partial class ActorSkillCooldownController : Node
{
	public event Action<SkillDefinition, float>? SkillCooldownStarted;
	public event Action<string>? SkillCooldownEnded;
	public event Action<float>? GlobalCooldownStarted;
	public event Action? GlobalCooldownEnded;

	private Node? _componentsRoot;
	private ActorSkillResourceController? _resourceController;
	private readonly Dictionary<string, CooldownRuntimeState> _skillCooldowns = new();
	private readonly Dictionary<string, CooldownRuntimeState> _groupCooldowns = new();
	private readonly List<ICooldownModifierSource> _modifierSources = new();
	private float _remainingGlobalCooldown;
	private bool _wasGlobalCooldownActive;

	public override void _Ready()
	{
		_componentsRoot = GetParent();
		_resourceController = _componentsRoot?.GetNodeOrNull<ActorSkillResourceController>("SkillResources");
		RefreshModifierSources();
		SetPhysicsProcess(true);
	}

	public override void _PhysicsProcess(double delta)
	{
		foreach (CooldownRuntimeState state in _skillCooldowns.Values)
		{
			bool wasActive = !state.IsReady;
			state.Tick(delta);
			if (wasActive && state.IsReady)
			{
				SkillCooldownEnded?.Invoke(state.Id);
			}
		}

		foreach (CooldownRuntimeState state in _groupCooldowns.Values)
		{
			state.Tick(delta);
		}

		_wasGlobalCooldownActive = _remainingGlobalCooldown > 0.0f;
		_remainingGlobalCooldown = Mathf.Max(0.0f, _remainingGlobalCooldown - (float)delta);
		if (_wasGlobalCooldownActive && _remainingGlobalCooldown <= 0.0f)
		{
			GlobalCooldownEnded?.Invoke();
		}
	}

	public SkillCastCheckResult CheckCast(SkillDefinition skill)
	{
		float remainingSkillCooldown = GetRemainingCooldown(skill);
		float remainingGroupCooldown = GetRemainingGroupCooldown(skill.CooldownGroupId);
		float remainingGlobalCooldown = GetRemainingGlobalCooldown();
		float currentResource = _resourceController?.CurrentResource ?? 0.0f;
		float requiredResource = Mathf.Max(0.0f, skill.ResourceCost);
		SkillCastBlockReason blockReason = ResolveBlockReason(
			skill,
			remainingSkillCooldown,
			remainingGroupCooldown,
			remainingGlobalCooldown,
			currentResource,
			requiredResource,
			_resourceController);

		return new SkillCastCheckResult
		{
			CanCast = blockReason == SkillCastBlockReason.None,
			BlockReason = blockReason,
			RemainingSkillCooldown = remainingSkillCooldown,
			RemainingGroupCooldown = remainingGroupCooldown,
			RemainingGlobalCooldown = skill.IgnoreGlobalCooldown ? 0.0f : remainingGlobalCooldown,
			CurrentResource = currentResource,
			RequiredResource = requiredResource
		};
	}

	public bool IsSkillReady(SkillDefinition skill)
	{
		return CheckCast(skill).CanCast;
	}

	public float GetRemainingCooldown(SkillDefinition skill)
	{
		return GetRemainingCooldown(skill.SkillId);
	}

	public float GetRemainingCooldown(string skillId)
	{
		if (_skillCooldowns.TryGetValue(skillId, out CooldownRuntimeState? state))
		{
			return state.RemainingSeconds;
		}

		return 0.0f;
	}

	public float GetRemainingGroupCooldown(string groupId)
	{
		if (string.IsNullOrWhiteSpace(groupId))
		{
			return 0.0f;
		}

		if (_groupCooldowns.TryGetValue(groupId, out CooldownRuntimeState? state))
		{
			return state.RemainingSeconds;
		}

		return 0.0f;
	}

	public float GetRemainingGlobalCooldown()
	{
		return _remainingGlobalCooldown;
	}

	public void CommitCast(SkillDefinition skill)
	{
		if (_resourceController != null)
		{
			_resourceController.TrySpend(skill.ResourceCost);
		}

		float effectiveSkillCooldown = GetEffectiveSkillCooldown(skill);
		if (effectiveSkillCooldown > 0.0f)
		{
			GetOrCreateSkillState(skill.SkillId).Start(effectiveSkillCooldown);
			SkillCooldownStarted?.Invoke(skill, effectiveSkillCooldown);
		}

		if (!string.IsNullOrWhiteSpace(skill.CooldownGroupId) && effectiveSkillCooldown > 0.0f)
		{
			GetOrCreateGroupState(skill.CooldownGroupId).Start(effectiveSkillCooldown);
		}

		if (!skill.IgnoreGlobalCooldown)
		{
			float effectiveGlobalCooldown = GetEffectiveGlobalCooldown(skill);
			if (effectiveGlobalCooldown > 0.0f)
			{
				_remainingGlobalCooldown = effectiveGlobalCooldown;
				GlobalCooldownStarted?.Invoke(effectiveGlobalCooldown);
			}
		}
	}

	public void ReduceCooldown(string skillId, float seconds)
	{
		if (_skillCooldowns.TryGetValue(skillId, out CooldownRuntimeState? state))
		{
			state.Reduce(seconds);
			if (state.IsReady)
			{
				SkillCooldownEnded?.Invoke(skillId);
			}
		}
	}

	public void ReduceGroupCooldown(string groupId, float seconds)
	{
		if (_groupCooldowns.TryGetValue(groupId, out CooldownRuntimeState? state))
		{
			state.Reduce(seconds);
		}
	}

	public void ReduceAllCooldowns(float seconds)
	{
		foreach (KeyValuePair<string, CooldownRuntimeState> pair in _skillCooldowns)
		{
			pair.Value.Reduce(seconds);
			if (pair.Value.IsReady)
			{
				SkillCooldownEnded?.Invoke(pair.Key);
			}
		}

		foreach (CooldownRuntimeState state in _groupCooldowns.Values)
		{
			state.Reduce(seconds);
		}

		_remainingGlobalCooldown = Mathf.Max(0.0f, _remainingGlobalCooldown - Mathf.Max(0.0f, seconds));
	}

	public void RefreshCooldown(string skillId)
	{
		if (_skillCooldowns.TryGetValue(skillId, out CooldownRuntimeState? state))
		{
			state.Refresh();
			SkillCooldownEnded?.Invoke(skillId);
		}
	}

	public void RefreshGroupCooldown(string groupId)
	{
		if (_groupCooldowns.TryGetValue(groupId, out CooldownRuntimeState? state))
		{
			state.Refresh();
		}
	}

	public void RefreshAllCooldowns()
	{
		foreach (KeyValuePair<string, CooldownRuntimeState> pair in _skillCooldowns)
		{
			pair.Value.Refresh();
			SkillCooldownEnded?.Invoke(pair.Key);
		}

		foreach (CooldownRuntimeState state in _groupCooldowns.Values)
		{
			state.Refresh();
		}

		_remainingGlobalCooldown = 0.0f;
		GlobalCooldownEnded?.Invoke();
	}

	private float GetEffectiveSkillCooldown(SkillDefinition skill)
	{
		RefreshModifierSources();
		float cooldown = Mathf.Max(0.0f, skill.BaseCooldownSeconds);
		float flatReduction = 0.0f;
		float multiplier = 1.0f;

		foreach (ICooldownModifierSource source in _modifierSources)
		{
			flatReduction += source.GetCooldownFlatReduction(skill);
			multiplier *= Mathf.Max(0.0f, source.GetCooldownMultiplier(skill));
		}

		return Mathf.Max(0.0f, (cooldown - flatReduction) * multiplier);
	}

	private float GetEffectiveGlobalCooldown(SkillDefinition skill)
	{
		RefreshModifierSources();
		float cooldown = Mathf.Max(0.0f, skill.BaseGlobalCooldownSeconds);
		float flatReduction = 0.0f;
		float multiplier = 1.0f;

		foreach (ICooldownModifierSource source in _modifierSources)
		{
			flatReduction += source.GetGlobalCooldownFlatReduction(skill);
			multiplier *= Mathf.Max(0.0f, source.GetGlobalCooldownMultiplier(skill));
		}

		return Mathf.Max(0.0f, (cooldown - flatReduction) * multiplier);
	}

	private CooldownRuntimeState GetOrCreateSkillState(string skillId)
	{
		if (!_skillCooldowns.TryGetValue(skillId, out CooldownRuntimeState? state))
		{
			state = new CooldownRuntimeState(skillId);
			_skillCooldowns[skillId] = state;
		}

		return state;
	}

	private CooldownRuntimeState GetOrCreateGroupState(string groupId)
	{
		if (!_groupCooldowns.TryGetValue(groupId, out CooldownRuntimeState? state))
		{
			state = new CooldownRuntimeState(groupId);
			_groupCooldowns[groupId] = state;
		}

		return state;
	}

	private void RefreshModifierSources()
	{
		_modifierSources.Clear();
		CollectModifierSources(_componentsRoot);
	}

	private static SkillCastBlockReason ResolveBlockReason(
		SkillDefinition skill,
		float remainingSkillCooldown,
		float remainingGroupCooldown,
		float remainingGlobalCooldown,
		float currentResource,
		float requiredResource,
		ActorSkillResourceController? resourceController)
	{
		if (remainingSkillCooldown > 0.0f)
		{
			return SkillCastBlockReason.SkillCooldown;
		}

		if (remainingGroupCooldown > 0.0f)
		{
			return SkillCastBlockReason.CooldownGroup;
		}

		if (!skill.IgnoreGlobalCooldown && remainingGlobalCooldown > 0.0f)
		{
			return SkillCastBlockReason.GlobalCooldown;
		}

		if (resourceController != null && requiredResource > 0.0f && currentResource + 0.0001f < requiredResource)
		{
			return SkillCastBlockReason.ResourceInsufficient;
		}

		return SkillCastBlockReason.None;
	}

	private void CollectModifierSources(Node? node)
	{
		if (node == null)
		{
			return;
		}

		if (node is ICooldownModifierSource source)
		{
			_modifierSources.Add(source);
		}

		foreach (Node child in node.GetChildren())
		{
			CollectModifierSources(child);
		}
	}
}
