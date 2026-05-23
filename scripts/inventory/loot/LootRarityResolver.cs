using BattleHarvesterStudy.Items;

namespace BattleHarvesterStudy.Inventory;

public static class LootRarityResolver
{
	public static LootRarity Resolve(ItemDefinition definition)
	{
		if (definition.LootRarity != LootRarity.None)
		{
			return definition.LootRarity;
		}

		return ItemValueClassifier.GetBand(definition) switch
		{
			ItemValueBand.White => LootRarity.White,
			ItemValueBand.Green => LootRarity.Green,
			ItemValueBand.Blue => LootRarity.Blue,
			ItemValueBand.Purple => LootRarity.Purple,
			ItemValueBand.Gold => LootRarity.Gold,
			ItemValueBand.Red => LootRarity.Red,
			_ => LootRarity.White,
		};
	}
}
