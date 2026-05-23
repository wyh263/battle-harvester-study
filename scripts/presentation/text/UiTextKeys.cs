namespace BattleHarvesterStudy.Presentation;

public static class UiTextKeys
{
	public static class Hud
	{
		public const string Health = "hud.health";
		public const string HealthDead = "hud.health_dead";
		public const string Armor = "hud.armor";
		public const string Target = "hud.target";
		public const string TargetFree = "hud.target_free";
		public const string TargetNone = "hud.target_none";
		public const string TargetDead = "hud.target_dead";
		public const string Ready = "hud.ready";
		public const string Seconds = "hud.seconds";
		public const string SkillSlot = "hud.skill_slot";
		public const string Resource = "hud.resource";
	}

	public static class Inventory
	{
		public const string HeaderPlayer = "inventory.header_player";
		public const string HeaderContainer = "inventory.header_container";
		public const string HeaderEquipment = "inventory.header_equipment";
		public const string SummaryUnconfigured = "inventory.summary_unconfigured";
		public const string SummaryClosed = "inventory.summary_closed";
		public const string SelectedEmpty = "inventory.selected_empty";
		public const string SelectedItem = "inventory.selected_item";
		public const string SelectedUnsearched = "inventory.selected_unsearched";
		public const string RunLoot = "inventory.run_loot";
		public const string PlayerSummary = "inventory.player_summary";
		public const string ContainerSummary = "inventory.container_summary";
		public const string WarehouseModeInactive = "inventory.warehouse_mode_inactive";
		public const string WarehouseModeActive = "inventory.warehouse_mode_active";
		public const string SellButton = "inventory.sell_button";
		public const string StopSellingButton = "inventory.stop_selling_button";
		public const string SellSelectedButton = "inventory.sell_selected_button";
		public const string EquipmentSummary = "inventory.equipment_summary";
		public const string DetailsEmpty = "inventory.details_empty";
		public const string DetailsTitle = "inventory.details_title";
		public const string DetailsBody = "inventory.details_body";
		public const string DetailsEquipmentNone = "inventory.details_equipment_none";
		public const string DetailsEquipmentBlock = "inventory.details_equipment_block";
		public const string DetailsUsableNone = "inventory.details_usable_none";
		public const string DetailsUsableBlock = "inventory.details_usable_block";
		public const string StatusCannotAccess = "inventory.status_cannot_access";
		public const string StatusOpenedContainer = "inventory.status_opened_container";
		public const string StatusClosedContainer = "inventory.status_closed_container";
		public const string StatusOpenedInventory = "inventory.status_opened_inventory";
		public const string StatusClosedInventory = "inventory.status_closed_inventory";
		public const string StatusQuickTransferSuccess = "inventory.status_quick_transfer_success";
		public const string StatusQuickTransferFailure = "inventory.status_quick_transfer_failure";
		public const string StatusTakeAll = "inventory.status_take_all";
		public const string StatusMoveSuccess = "inventory.status_move_success";
		public const string StatusMoveFailure = "inventory.status_move_failure";
		public const string StatusEquipSuccess = "inventory.status_equip_success";
		public const string StatusEquipFailure = "inventory.status_equip_failure";
		public const string StatusUnequipSuccess = "inventory.status_unequip_success";
		public const string StatusUnequipFailure = "inventory.status_unequip_failure";
		public const string StatusUseSuccess = "inventory.status_use_success";
		public const string StatusUseFailure = "inventory.status_use_failure";
		public const string StatusSoldItems = "inventory.status_sold_items";
		public const string StatusWeaponSkillEquipped = "inventory.status_weapon_skill_equipped";
		public const string StatusWeaponSkillEquipFailed = "inventory.status_weapon_skill_equip_failed";
		public const string StatusWeaponSkillRemoved = "inventory.status_weapon_skill_removed";
		public const string StatusSecureInsurancePurchased = "inventory.status_secure_insurance_purchased";
		public const string StatusSecureInsuranceFailed = "inventory.status_secure_insurance_failed";
		public const string StatusSecureStoreFailed = "inventory.status_secure_store_failed";
		public const string SecureContainerHeader = "inventory.secure_container_header";
		public const string SecureInsuranceUninitialized = "inventory.secure_insurance_uninitialized";
		public const string SecureInsuranceRentalActive = "inventory.secure_insurance_rental_active";
		public const string SecureInsuranceChallengeActive = "inventory.secure_insurance_challenge_active";
		public const string SecureInsuranceRetrievalOnly = "inventory.secure_insurance_retrieval_only";
		public const string SecureInsuranceNone = "inventory.secure_insurance_none";
		public const string SecureQuotaButton = "inventory.secure_quota_button";
		public const string SecureRentalButton = "inventory.secure_rental_button";
		public const string SecureRentalExtendButton = "inventory.secure_rental_extend_button";
		public const string SecureRentalType = "inventory.secure_rental_type";
		public const string InsufficientCredits = "inventory.insufficient_credits";
		public const string DragHint = "inventory.drag_hint";
		public const string HintToggleInventory = "inventory.hint_toggle_inventory";
		public const string HintTakeAll = "inventory.hint_take_all";
		public const string HintQuickTransfer = "inventory.hint_quick_transfer";
		public const string HintDragItem = "inventory.hint_drag_item";
		public const string HintRotateDragging = "inventory.hint_rotate_dragging";
		public const string HintCloseContainer = "inventory.hint_close_container";
		public const string HintInspectSlot = "inventory.hint_inspect_slot";
		public const string HintDragToEquip = "inventory.hint_drag_to_equip";
		public const string HintUseItem = "inventory.hint_use_item";
		public const string AccessMissingRequester = "inventory.access_missing_requester";
		public const string AccessLocked = "inventory.access_locked";
		public const string AccessSingleUseConsumed = "inventory.access_single_use_consumed";
		public const string AccessOutOfRange = "inventory.access_out_of_range";
		public const string AccessMissingTag = "inventory.access_missing_tag";
	}

	public static class Status
	{
		public const string PlayerHeader = "status.player_header";
		public const string Firearm = "status.firearm";
		public const string Hit = "status.hit";
		public const string Magazine = "status.magazine";
		public const string Aim = "status.aim";
		public const string On = "status.on";
		public const string Off = "status.off";
		public const string FireMode = "status.fire_mode";
		public const string FireModeAutomatic = "status.fire_mode_automatic";
		public const string FireModeSelective = "status.fire_mode_selective";
		public const string FireModeSingleShot = "status.fire_mode_single_shot";
		public const string Health = "status.health";
		public const string Stamina = "status.stamina";
		public const string Attack = "status.attack";
		public const string ActiveWeapon = "status.active_weapon";
		public const string Defense = "status.defense";
		public const string MoveSpeed = "status.move_speed";
		public const string SearchMultiplier = "status.search_multiplier";
		public const string HighRarity = "status.high_rarity";
		public const string ActiveSearchMods = "status.active_search_mods";
	}

	public static class World
	{
		public const string ContainerUnavailable = "world.container_unavailable";
		public const string ContainerLocked = "world.container_locked";
		public const string ContainerConsumed = "world.container_consumed";
		public const string ContainerOpenable = "world.container_openable";
		public const string ContainerLabel = "world.container_label";
	}
}
