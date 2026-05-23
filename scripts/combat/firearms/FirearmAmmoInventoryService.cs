using System.Collections.Generic;
using System.Linq;
using BattleHarvesterStudy.Inventory;
using BattleHarvesterStudy.Items;

namespace BattleHarvesterStudy.Combat.Firearms;

public static class FirearmAmmoInventoryService
{
	public static int GetReserveAmmoCount(InventoryComponent? inventory, AmmoType ammoType)
	{
		if (inventory == null || ammoType == AmmoType.None)
		{
			return 0;
		}

		int total = 0;
		foreach (GridContainerComponent container in inventory.Containers)
		{
			foreach (ContainerItemRecord record in container.ItemRecords)
			{
				if (record.Item.Definition.Ammo?.AmmoType == ammoType)
				{
					total += record.Item.StackCount;
				}
			}
		}

		return total;
	}

	public static bool TryReloadBestAvailable(
		InventoryComponent? inventory,
		ItemInstance firearmItem,
		FirearmResolvedStats resolved,
		out AmmoReloadResult result)
	{
		result = Empty;
		if (inventory == null)
		{
			return false;
		}

		int magazineCapacity = resolved.MagazineCapacity;
		int missingAmmo = magazineCapacity - firearmItem.CurrentMagazineAmmo;
		if (missingAmmo <= 0)
		{
			return false;
		}

		List<AmmoRecord> candidates = GatherAmmoRecords(inventory, resolved.AmmoType);
		if (candidates.Count == 0)
		{
			return false;
		}

		string preferredItemId = firearmItem.LoadedAmmoItemId;
		AmmoItemDefinition? selectedAmmo = SelectPreferredAmmoDefinition(candidates, preferredItemId);
		if (selectedAmmo == null)
		{
			return false;
		}

		string selectedItemId = AmmoItemDefinition.BuildDefaultAmmoItemId(selectedAmmo.AmmoType, selectedAmmo.AmmoTier);
		ItemDefinition? firstDefinition = candidates
			.FirstOrDefault(candidate => candidate.Item.Definition.ItemId == selectedItemId)?.Item.Definition;
		if (firstDefinition == null)
		{
			firstDefinition = candidates
				.Where(candidate => candidate.Item.Definition.Ammo?.AmmoType == selectedAmmo.AmmoType
					&& candidate.Item.Definition.Ammo?.AmmoTier == selectedAmmo.AmmoTier)
				.Select(candidate => candidate.Item.Definition)
				.FirstOrDefault();
		}

		if (firstDefinition == null)
		{
			return false;
		}

		int loadedAmount = ConsumeAmmoAcrossStacks(candidates, firstDefinition.ItemId, missingAmmo);
		if (loadedAmount <= 0)
		{
			return false;
		}

		firearmItem.LoadMagazine(
			firearmItem.CurrentMagazineAmmo + loadedAmount,
			selectedAmmo,
			firstDefinition.ItemId,
			magazineCapacity);

		result = new AmmoReloadResult(
			true,
			loadedAmount,
			GetReserveAmmoCount(inventory, resolved.AmmoType),
			firstDefinition,
			selectedAmmo);
		return true;
	}

	private static List<AmmoRecord> GatherAmmoRecords(InventoryComponent inventory, AmmoType ammoType)
	{
		List<AmmoRecord> records = [];
		foreach (GridContainerComponent container in inventory.Containers)
		{
			foreach (ContainerItemRecord record in container.ItemRecords)
			{
				AmmoItemDefinition? ammo = record.Item.Definition.Ammo;
				if (ammo == null || ammo.AmmoType != ammoType || record.Item.StackCount <= 0)
				{
					continue;
				}

				records.Add(new AmmoRecord(container, record));
			}
		}

		return records;
	}

	private static AmmoItemDefinition? SelectPreferredAmmoDefinition(IEnumerable<AmmoRecord> candidates, string preferredItemId)
	{
		if (!string.IsNullOrWhiteSpace(preferredItemId))
		{
			AmmoItemDefinition? matching = candidates
				.Where(candidate => candidate.Item.Definition.ItemId == preferredItemId)
				.Select(candidate => candidate.Item.Definition.Ammo)
				.FirstOrDefault(ammo => ammo != null);
			if (matching != null)
			{
				return matching;
			}
		}

		return candidates
			.Select(candidate => candidate.Item.Definition.Ammo)
			.Where(ammo => ammo != null)
			.OrderByDescending(ammo => ammo!.AmmoTier)
			.ThenByDescending(ammo => ammo!.PenetrationTier)
			.FirstOrDefault();
	}

	private static int ConsumeAmmoAcrossStacks(List<AmmoRecord> candidates, string itemId, int amount)
	{
		int remaining = amount;
		int consumed = 0;
		foreach (AmmoRecord candidate in candidates.Where(candidate => candidate.Item.Definition.ItemId == itemId).ToList())
		{
			if (remaining <= 0)
			{
				break;
			}

			int taken = System.Math.Min(remaining, candidate.Item.StackCount);
			if (!candidate.Container.TryConsumeItemStack(candidate.Item.InstanceId, taken))
			{
				continue;
			}

			remaining -= taken;
			consumed += taken;
		}

		return consumed;
	}

	private sealed record AmmoRecord(GridContainerComponent Container, ContainerItemRecord Record)
	{
		public ItemInstance Item => Record.Item;
	}

	public readonly record struct AmmoReloadResult(
		bool Succeeded,
		int LoadedAmount,
		int RemainingReserveAmmo,
		ItemDefinition? LoadedAmmoDefinition,
		AmmoItemDefinition? LoadedAmmo);

	public static readonly AmmoReloadResult Empty = new(false, 0, 0, null, null);
}
