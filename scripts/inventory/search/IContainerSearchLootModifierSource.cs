namespace BattleHarvesterStudy.Inventory.Search;

public interface IContainerSearchLootModifierSource
{
	float GetSearchSpeedMultiplier(WorldContainer container);
	float GetLootPoolWeightMultiplier(WorldContainer container, LootCategoryPoolDefinition pool);
	float GetLootRarityWeightMultiplier(WorldContainer container, LootRarity rarity);
}
