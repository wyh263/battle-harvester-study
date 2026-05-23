using Godot;
using System.Collections.Generic;
using System.Linq;
using BattleHarvesterStudy.Items;
using BattleHarvesterStudy.Inventory.Search;

namespace BattleHarvesterStudy.Presentation;

public partial class ItemDetailsPanelPresenter : Node
{
	public static string BuildSummaryText(ContainerItemRecord? selectedRecord)
	{
		if (selectedRecord == null)
		{
			return UiText.Resolve(UiTextKeys.Inventory.DetailsEmpty);
		}

		bool chinese = UiText.CurrentLocale == UiText.DefaultLocale;
		ItemDefinition definition = selectedRecord.Item.Definition;
		float valuePerCell = ItemValueClassifier.GetValuePerCell(definition);
		string valueBand = ItemValueClassifier.GetBandLabel(definition);
		string tags = ContentTextFormatter.FormatTags(definition.Tags.Cast<string>());
		string rotated = EquipmentTextFormatter.GetBooleanText(selectedRecord.Item.IsRotated);
		EquipmentDefinition? equipment = definition.Equipment;
		UsableItemDefinition? usable = definition.Usable;
		WeaponSkillItemDefinition? weaponSkill = definition.WeaponSkill;
		AmmoItemDefinition? ammo = definition.Ammo;

		string equipmentBlock = equipment == null
			? UiText.Resolve(UiTextKeys.Inventory.DetailsEquipmentNone)
			: EquipmentTextFormatter.FormatEquipmentDetails(selectedRecord.Item);

		string usableBlock = usable == null
			? UiText.Resolve(UiTextKeys.Inventory.DetailsUsableNone)
			: UiText.Resolve(
				UiTextKeys.Inventory.DetailsUsableBlock,
				("health", usable.RestoreHealth),
				("resource", usable.RestoreResource),
				("consume_on_use", usable.RepairsArmor
					? $"{EquipmentTextFormatter.GetBooleanText(usable.ConsumeOnUse)} / Repair T{usable.RepairTier}"
					: EquipmentTextFormatter.GetBooleanText(usable.ConsumeOnUse)),
				("allow_use_at_full", EquipmentTextFormatter.GetBooleanText(usable.AllowUseAtFull)));

		string modifierBlock = usable?.ContainerModifierProfile == null
			? string.Empty
			: BuildContainerModifierBlock(usable.ContainerModifierProfile, chinese);

		string weaponSkillBlock = weaponSkill == null
			? string.Empty
			: BuildWeaponSkillBlock(selectedRecord.Item, weaponSkill, chinese);
		string ammoBlock = ammo == null
			? string.Empty
			: BuildAmmoBlock(ammo, chinese);

		return UiText.Resolve(
			UiTextKeys.Inventory.DetailsBody,
			("title", UiText.Resolve(UiTextKeys.Inventory.DetailsTitle)),
			("name", ContentTextFormatter.GetItemDisplayName(definition)),
			("category", $"{GetCategoryDisplayName(definition.Category, chinese)} / {valueBand}"),
			("stack", selectedRecord.Item.StackCount),
			("max_stack", definition.MaxStack),
			("width", selectedRecord.Item.GetFootprintWidth()),
			("height", selectedRecord.Item.GetFootprintHeight()),
			("rotated", rotated),
			("value", $"{definition.BaseValue} ({valuePerCell:0.#}/cell)"),
			("tags", tags),
			("equipment_block", string.Join("\n", new[] { equipmentBlock, usableBlock, modifierBlock, weaponSkillBlock, ammoBlock }.Where(text => !string.IsNullOrWhiteSpace(text)))));
	}

	public static void RefreshWeaponSkillButtons(Control? window, ItemInstance? item)
	{
		VBoxContainer? slotsContainer = window?.GetNodeOrNull<VBoxContainer>("Margin/VBox/WeaponSkillSlots");
		if (slotsContainer == null)
		{
			return;
		}

		List<Button> buttons = [];
		foreach (Node child in slotsContainer.GetChildren())
		{
			if (child is Button button)
			{
				buttons.Add(button);
			}
		}

		int slotCount = item?.GetWeaponSkillSlotCount() ?? 0;
		slotsContainer.Visible = slotCount > 0;
		bool chinese = UiText.CurrentLocale == UiText.DefaultLocale;
		for (int index = 0; index < buttons.Count; index++)
		{
			Button button = buttons[index];
			bool visible = index < slotCount;
			button.Visible = visible;
			button.Disabled = !visible;
			if (!visible)
			{
				continue;
			}

			InstalledWeaponSkillState? state = item?.GetInstalledWeaponSkillState(index);
			if (state?.Skill == null)
			{
				button.Text = chinese
					? $"武器技能槽 {index + 1}\n空槽"
					: $"Weapon Skill {index + 1}\nEmpty";
				continue;
			}

			string suffix = state.PermanentlyUnlocked
				? (chinese ? "已习得" : "Learned")
				: (chinese ? $"次数 {state.RemainingUses}" : $"Uses {state.RemainingUses}");
			button.Text = $"{index + 1}. {ContentTextFormatter.GetSkillDisplayName(state.Skill)}\n{suffix}";
		}
	}

	private static string BuildWeaponSkillBlock(ItemInstance item, WeaponSkillItemDefinition weaponSkill, bool chinese)
	{
		SkillDefinition? skill = weaponSkill.Skill;
		string skillName = ContentTextFormatter.GetSkillDisplayName(skill);
		string compatibility = skill?.GetWeaponCompatibilitySummary(chinese) ?? "-";
		int currentUses = item.RemainingUses > 0 ? item.RemainingUses : weaponSkill.GrantedUses;
		string unlockSummary = weaponSkill.CanBeLearned
			? (chinese
				? $"使用 {weaponSkill.RequiredUseCount} / 击杀 {weaponSkill.RequiredKillCount} / Boss {weaponSkill.RequiredBossKillCount}"
				: $"Uses {weaponSkill.RequiredUseCount} / Kills {weaponSkill.RequiredKillCount} / Boss {weaponSkill.RequiredBossKillCount}")
			: (chinese ? "不可永久习得" : "No permanent unlock");

		return chinese
			? $"武器技能物品\n技能  {skillName}\n次数  {currentUses}\n适配  {compatibility}\n习得  {unlockSummary}"
			: $"Weapon Skill Item\nSkill  {skillName}\nUses  {currentUses}\nCompat  {compatibility}\nUnlock  {unlockSummary}";
	}

	private static string BuildContainerModifierBlock(ContainerModifierProfileDefinition profile, bool chinese)
	{
		List<string> lines =
		[
			chinese ? "搜索修正" : "Search Modifier",
			chinese ? $"效果  {profile.DisplayName}" : $"Effect  {profile.DisplayName}"
		];

		if (!Mathf.IsEqualApprox(profile.GlobalSearchSpeedMultiplier, 1.0f))
		{
			lines.Add(chinese
				? $"搜索速度  x{profile.GlobalSearchSpeedMultiplier:0.##}"
				: $"Search Speed  x{profile.GlobalSearchSpeedMultiplier:0.##}");
		}

		if (!Mathf.IsEqualApprox(profile.GlobalLootWeightMultiplier, 1.0f))
		{
			lines.Add(chinese
				? $"爆率倍率  x{profile.GlobalLootWeightMultiplier:0.##}"
				: $"Loot Mult  x{profile.GlobalLootWeightMultiplier:0.##}");
		}

		return string.Join("\n", lines);
	}

	private static string BuildAmmoBlock(AmmoItemDefinition ammo, bool chinese)
	{
		return chinese
			? $"弹药信息\n类型  {FirearmTextFormatter.GetAmmoTypeName(ammo.AmmoType, true)}\n等级  T{ammo.AmmoTier}\nPT  {ammo.PenetrationTier:0.##}"
			: $"Ammo Info\nType  {FirearmTextFormatter.GetAmmoTypeName(ammo.AmmoType, false)}\nTier  T{ammo.AmmoTier}\nPT  {ammo.PenetrationTier:0.##}";
	}

	private static string GetCategoryDisplayName(ItemCategory category, bool chinese)
	{
		return category switch
		{
			ItemCategory.Generic => chinese ? "通用" : "Generic",
			ItemCategory.Weapon => chinese ? "武器" : "Weapon",
			ItemCategory.Armor => chinese ? "护甲" : "Armor",
			ItemCategory.Medical => chinese ? "医疗" : "Medical",
			ItemCategory.Consumable => chinese ? "消耗品" : "Consumable",
			ItemCategory.Ammo => chinese ? "弹药" : "Ammo",
			ItemCategory.Material => chinese ? "材料" : "Material",
			ItemCategory.KeyItem => chinese ? "关键物品" : "Key Item",
			ItemCategory.Valuable => chinese ? "贵重品" : "Valuable",
			ItemCategory.Container => chinese ? "容器" : "Container",
			ItemCategory.Gloves => chinese ? "手套" : "Gloves",
			ItemCategory.WeaponSkill => chinese ? "武器技能" : "Weapon Skill",
			_ => category.ToString()
		};
	}
}
