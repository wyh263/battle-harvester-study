using Godot;
using BattleHarvesterStudy.Equipment;

namespace BattleHarvesterStudy.Items;

public static class ItemValueClassifier
{
	public static float GetValuePerCell(ItemDefinition definition)
	{
		int area = Mathf.Max(1, definition.GridWidth * definition.GridHeight);
		return definition.BaseValue / (float)area;
	}

	public static int GetMarketValue(ItemInstance item)
	{
		return Mathf.Max(0, item.Definition.BaseValue) * Mathf.Max(1, item.StackCount);
	}

	public static int GetRunLootMarketValue(ItemInstance item)
	{
		if (!item.CountsAsRunLoot)
		{
			return 0;
		}

		int countedStacks = item.Definition.MaxStack > 1
			? item.RunLootStackCount
			: Mathf.Max(1, item.RunLootStackCount);
		return Mathf.Max(0, item.Definition.BaseValue) * Mathf.Max(0, countedStacks);
	}

	public static ItemValueBand GetBand(ItemDefinition definition)
	{
		if (definition.LootRarity != LootRarity.None)
		{
			return GetRarityBand(definition.LootRarity);
		}

		if (definition.Equipment is EquipmentDefinition equipment)
		{
			return GetTierBand(equipment.Tier);
		}

		float valuePerCell = GetValuePerCell(definition);
		return valuePerCell switch
		{
			<= 5000.0f => ItemValueBand.White,
			<= 10000.0f => ItemValueBand.Green,
			<= 20000.0f => ItemValueBand.Blue,
			<= 50000.0f => ItemValueBand.Purple,
			<= 200000.0f => ItemValueBand.Gold,
			_ => ItemValueBand.Red,
		};
	}

	public static string GetBandLabel(ItemDefinition definition)
	{
		bool chinese = Presentation.UiText.CurrentLocale == Presentation.UiText.DefaultLocale;
		return GetBand(definition) switch
		{
			ItemValueBand.White => chinese ? "白" : "White",
			ItemValueBand.Green => chinese ? "绿" : "Green",
			ItemValueBand.Blue => chinese ? "蓝" : "Blue",
			ItemValueBand.Purple => chinese ? "紫" : "Purple",
			ItemValueBand.Gold => chinese ? "金" : "Gold",
			ItemValueBand.Red => chinese ? "红" : "Red",
			_ => chinese ? "未知" : "Unknown",
		};
	}

	public static Color GetFillColor(ItemDefinition definition)
	{
		return GetBand(definition) switch
		{
			ItemValueBand.White => new Color(0.91f, 0.93f, 0.96f, 0.72f),
			ItemValueBand.Green => new Color(0.45f, 0.60f, 0.46f, 0.68f),
			ItemValueBand.Blue => new Color(0.40f, 0.56f, 0.74f, 0.68f),
			ItemValueBand.Purple => new Color(0.58f, 0.45f, 0.70f, 0.70f),
			ItemValueBand.Gold => new Color(0.76f, 0.64f, 0.34f, 0.72f),
			ItemValueBand.Red => new Color(0.76f, 0.36f, 0.36f, 0.74f),
			_ => new Color(0.70f, 0.74f, 0.78f, 0.70f),
		};
	}

	public static Color GetBorderColor(ItemDefinition definition)
	{
		return GetBand(definition) switch
		{
			ItemValueBand.White => new Color(0.98f, 0.99f, 1.0f, 0.95f),
			ItemValueBand.Green => new Color(0.71f, 0.90f, 0.67f, 0.95f),
			ItemValueBand.Blue => new Color(0.67f, 0.83f, 0.98f, 0.95f),
			ItemValueBand.Purple => new Color(0.86f, 0.74f, 0.98f, 0.95f),
			ItemValueBand.Gold => new Color(0.97f, 0.86f, 0.55f, 0.95f),
			ItemValueBand.Red => new Color(0.98f, 0.63f, 0.63f, 0.96f),
			_ => Colors.White,
		};
	}

	public static Color GetTextColor(ItemDefinition definition)
	{
		return GetBand(definition) switch
		{
			ItemValueBand.White => new Color(0.18f, 0.20f, 0.25f, 1.0f),
			ItemValueBand.Gold => new Color(0.18f, 0.12f, 0.04f, 1.0f),
			ItemValueBand.Red => new Color(1.0f, 0.93f, 0.93f, 1.0f),
			_ => new Color(0.95f, 0.97f, 1.0f, 1.0f),
		};
	}

	private static ItemValueBand GetTierBand(int tier)
	{
		return Mathf.Max(1, tier) switch
		{
			1 => ItemValueBand.White,
			2 => ItemValueBand.Green,
			3 => ItemValueBand.Blue,
			4 => ItemValueBand.Purple,
			5 => ItemValueBand.Gold,
			_ => ItemValueBand.Red,
		};
	}

	private static ItemValueBand GetRarityBand(LootRarity rarity)
	{
		return rarity switch
		{
			LootRarity.Green => ItemValueBand.Green,
			LootRarity.Blue => ItemValueBand.Blue,
			LootRarity.Purple => ItemValueBand.Purple,
			LootRarity.Gold => ItemValueBand.Gold,
			LootRarity.Red => ItemValueBand.Red,
			_ => ItemValueBand.White,
		};
	}
}

