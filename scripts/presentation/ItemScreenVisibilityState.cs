namespace BattleHarvesterStudy.Presentation;

public readonly record struct ItemScreenVisibilityState(
	bool RootVisible,
	bool PlayerPanelVisible,
	bool ExternalContainerPanelVisible,
	bool SecureContainerPanelVisible,
	bool DetailsPanelVisible,
	bool EquipmentPanelVisible);
