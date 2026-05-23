namespace BattleHarvesterStudy.Inventory.Search;

public readonly record struct ContainerSearchVisualState(
	bool UsesSearchRules,
	bool IsRevealed,
	bool IsSearching);
