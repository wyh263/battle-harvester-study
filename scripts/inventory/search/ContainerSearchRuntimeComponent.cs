using Godot;
using System.Linq;
using BattleHarvesterStudy.Items;

namespace BattleHarvesterStudy.Inventory.Search;

public partial class ContainerSearchRuntimeComponent : Node
{
	[Signal]
	public delegate void SearchStateChangedEventHandler();

	[Export]
	public NodePath ContainerPath { get; set; } = "../GridContainer";

	[Export]
	public NodePath LootComponentPath { get; set; } = "../ContainerLoot";

	[Export]
	public bool SearchEnabled { get; set; } = true;

	[Export(PropertyHint.Range, "0.1,10.0,0.1")]
	public float SearchSpeedMultiplier { get; set; } = 1.0f;

	private GridContainerComponent? _container;
	private ContainerLootComponent? _lootComponent;
	private Node3D? _searchRequester;
	private readonly System.Collections.Generic.Dictionary<string, bool> _revealedStates = [];
	private string _currentSearchingInstanceId = string.Empty;
	private float _currentSearchElapsedSeconds;
	private bool _searchActive;

	public bool IsSearchActive => _searchActive;
	public bool HasActiveSearch => SearchEnabled && !string.IsNullOrWhiteSpace(_currentSearchingInstanceId);

	public override void _Ready()
	{
		_container = GetNodeOrNull<GridContainerComponent>(ContainerPath);
		_lootComponent = GetNodeOrNull<ContainerLootComponent>(LootComponentPath);
		if (_container != null)
		{
			_container.SearchRuntime = this;
			_container.ContainerChanged += OnContainerChanged;
		}

		if (_lootComponent != null)
		{
			_lootComponent.LootFilled += OnLootFilled;
		}

		SetProcess(true);
	}

	public override void _ExitTree()
	{
		if (_container != null)
		{
			_container.ContainerChanged -= OnContainerChanged;
			if (_container.SearchRuntime == this)
			{
				_container.SearchRuntime = null;
			}
		}

		if (_lootComponent != null)
		{
			_lootComponent.LootFilled -= OnLootFilled;
		}
	}

	public override void _Process(double delta)
	{
		if (!UsesSearchRules() || !_searchActive || _container == null)
		{
			return;
		}

		if (!TryGetCurrentSearchingRecord(out ContainerItemRecord? currentRecord) || currentRecord == null)
		{
			BeginNextSearchIfNeeded();
			return;
		}

		_currentSearchElapsedSeconds += (float)delta;
		float searchDuration = ResolveSearchDurationSeconds(currentRecord.Item.Definition);
		if (_currentSearchElapsedSeconds < searchDuration)
		{
			return;
		}

		_revealedStates[currentRecord.Item.InstanceId] = true;
		_currentSearchingInstanceId = string.Empty;
		_currentSearchElapsedSeconds = 0.0f;
		NotifySearchStateChanged();
		BeginNextSearchIfNeeded();
	}

	public void RegisterCurrentContainerItemsAsUnrevealed()
	{
		if (_container == null)
		{
			return;
		}

		if (!UsesSearchRules())
		{
			_revealedStates.Clear();
			_currentSearchingInstanceId = string.Empty;
			_currentSearchElapsedSeconds = 0.0f;
			NotifySearchStateChanged();
			return;
		}

		foreach (ContainerItemRecord record in _container.ItemRecords)
		{
			_revealedStates[record.Item.InstanceId] = false;
		}

		if (string.IsNullOrWhiteSpace(_currentSearchingInstanceId) || IsItemRevealed(_currentSearchingInstanceId))
		{
			_currentSearchingInstanceId = string.Empty;
			_currentSearchElapsedSeconds = 0.0f;
		}

		NotifySearchStateChanged();
		BeginNextSearchIfNeeded();
	}

	public void SetSearchActive(bool active)
	{
		if (_searchActive == active)
		{
			return;
		}

		_searchActive = active;
		if (!_searchActive)
		{
			ClearCurrentSearchProgress();
			return;
		}

		BeginNextSearchIfNeeded();
	}

	public void SetSearchRequester(Node3D? requester)
	{
		_searchRequester = requester;
	}

	public bool CanInteractWithItem(string instanceId)
	{
		return !UsesSearchRules() || IsItemRevealed(instanceId);
	}

	public bool IsItemRevealed(string instanceId)
	{
		return !_revealedStates.TryGetValue(instanceId, out bool revealed) || revealed;
	}

	public ContainerSearchVisualState GetVisualState(string instanceId)
	{
		bool usesSearchRules = UsesSearchRules() && _revealedStates.ContainsKey(instanceId);
		bool revealed = IsItemRevealed(instanceId);
		bool searching = UsesSearchRules() && instanceId == _currentSearchingInstanceId;
		return new ContainerSearchVisualState(usesSearchRules, revealed, searching);
	}

	private float ResolveSearchDurationSeconds(ItemDefinition definition)
	{
		float resolvedMultiplier = SearchSpeedMultiplier;
		if (GetParentOrNull<WorldContainer>() is WorldContainer container)
		{
			resolvedMultiplier *= ContainerModifierResolver.ResolveSearchSpeedMultiplier(_searchRequester, container);
		}

		return ContainerSearchResolver.ResolveSearchDurationSeconds(definition, resolvedMultiplier);
	}

	private void OnLootFilled(int addedItemCount, int rejectedItemCount)
	{
		RegisterCurrentContainerItemsAsUnrevealed();
	}

	private void OnContainerChanged()
	{
		PruneRemovedItems();
		if (_currentSearchingInstanceId.Length > 0 && IsItemRevealed(_currentSearchingInstanceId))
		{
			_currentSearchingInstanceId = string.Empty;
			_currentSearchElapsedSeconds = 0.0f;
		}

		if (_searchActive)
		{
			BeginNextSearchIfNeeded();
		}

		EmitSignal(SignalName.SearchStateChanged);
	}

	private void BeginNextSearchIfNeeded()
	{
		if (!_searchActive || _container == null || !UsesSearchRules() || !string.IsNullOrWhiteSpace(_currentSearchingInstanceId))
		{
			return;
		}

		ContainerItemRecord? nextRecord = _container.ItemRecords
			.Where(record => _revealedStates.TryGetValue(record.Item.InstanceId, out bool revealed) && !revealed)
			.OrderBy(record => record.Origin.Y)
			.ThenBy(record => record.Origin.X)
			.FirstOrDefault();
		if (nextRecord == null)
		{
			return;
		}

		_currentSearchingInstanceId = nextRecord.Item.InstanceId;
		_currentSearchElapsedSeconds = 0.0f;
		NotifySearchStateChanged();
	}

	private bool TryGetCurrentSearchingRecord(out ContainerItemRecord? record)
	{
		record = null;
		if (_container == null || string.IsNullOrWhiteSpace(_currentSearchingInstanceId) || !_container.ContainsItem(_currentSearchingInstanceId))
		{
			return false;
		}

		record = _container.GetRequiredRecord(_currentSearchingInstanceId);
		return true;
	}

	private void ClearCurrentSearchProgress()
	{
		if (string.IsNullOrWhiteSpace(_currentSearchingInstanceId) && _currentSearchElapsedSeconds <= 0.0f)
		{
			return;
		}

		_currentSearchingInstanceId = string.Empty;
		_currentSearchElapsedSeconds = 0.0f;
		NotifySearchStateChanged();
	}

	private void PruneRemovedItems()
	{
		if (_container == null)
		{
			_revealedStates.Clear();
			return;
		}

		System.Collections.Generic.List<string> staleInstanceIds = [];
		foreach (string instanceId in _revealedStates.Keys)
		{
			if (!_container.ContainsItem(instanceId))
			{
				staleInstanceIds.Add(instanceId);
			}
		}

		foreach (string instanceId in staleInstanceIds)
		{
			_revealedStates.Remove(instanceId);
		}
	}

	private void NotifySearchStateChanged()
	{
		_container?.NotifyVisualStateChanged();
		EmitSignal(SignalName.SearchStateChanged);
	}

	private bool UsesSearchRules()
	{
		if (!SearchEnabled || _container == null)
		{
			return false;
		}

		return _container.Definition?.RequiresSearch ?? true;
	}
}
