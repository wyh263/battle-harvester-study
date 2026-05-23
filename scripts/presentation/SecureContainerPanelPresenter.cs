using Godot;
using BattleHarvesterStudy.Session;

namespace BattleHarvesterStudy.Presentation;

public partial class SecureContainerPanelPresenter : Node
{
	[Export]
	public NodePath HeaderLabelPath { get; set; } = new("../../InventoryUi/PlayerInventoryWindow/Margin/VBox/ContentRow/InventoryColumn/SecureContainerPanel/Margin/SecureVBox/SecureHeader");

	[Export]
	public NodePath StatusLabelPath { get; set; } = new("../../InventoryUi/PlayerInventoryWindow/Margin/VBox/ContentRow/InventoryColumn/SecureContainerPanel/Margin/SecureVBox/SecureStatus");

	[Export]
	public NodePath RentalButtonPath { get; set; } = new("../../InventoryUi/PlayerInventoryWindow/Margin/VBox/ContentRow/InventoryColumn/SecureContainerPanel/Margin/SecureVBox/SecureActions/RentalButton");

	[Export]
	public NodePath QuotaButtonPath { get; set; } = new("../../InventoryUi/PlayerInventoryWindow/Margin/VBox/ContentRow/InventoryColumn/SecureContainerPanel/Margin/SecureVBox/SecureActions/QuotaButton");

	private Label? _headerLabel;
	private Label? _statusLabel;
	private Button? _rentalButton;
	private Button? _quotaButton;

	public override void _Ready()
	{
		_headerLabel = GetNodeOrNull<Label>(HeaderLabelPath);
		_statusLabel = GetNodeOrNull<Label>(StatusLabelPath);
		_rentalButton = GetNodeOrNull<Button>(RentalButtonPath);
		_quotaButton = GetNodeOrNull<Button>(QuotaButtonPath);

		if (_rentalButton != null)
		{
			_rentalButton.Pressed += OnRentalPressed;
		}

		UiText.LanguageChanged += OnLanguageChanged;
		SetProcess(true);
		Refresh();
	}

	public override void _ExitTree()
	{
		if (_rentalButton != null)
		{
			_rentalButton.Pressed -= OnRentalPressed;
		}

		UiText.LanguageChanged -= OnLanguageChanged;
	}

	public override void _Process(double delta)
	{
		Refresh();
	}

	private void OnLanguageChanged()
	{
		Refresh();
	}

	private void Refresh()
	{
		RunSession? runSession = RunSession.Instance;

		if (_headerLabel != null)
		{
			_headerLabel.Text = UiText.Resolve(UiTextKeys.Inventory.SecureContainerHeader);
		}

		if (_statusLabel != null)
		{
			_statusLabel.Text = BuildStatusText(runSession);
		}

		if (_rentalButton != null)
		{
			_rentalButton.Text = BuildRentalButtonText(runSession);
		}

		if (_quotaButton != null)
		{
			_quotaButton.Disabled = true;
			_quotaButton.Text = UiText.Resolve(UiTextKeys.Inventory.SecureQuotaButton);
		}
	}

	private void OnRentalPressed()
	{
		RunSession? runSession = RunSession.Instance;
		if (runSession == null)
		{
			return;
		}

		InventoryUiController? inventoryUiController = GetNodeOrNull<InventoryUiController>("../InventoryUiController");
		if (runSession.TryPurchaseSecureContainerRentalInsurance(out int chargedCredits))
		{
			inventoryUiController?.PublishStatus(
				UiTextKeys.Inventory.StatusSecureInsurancePurchased,
				UiTextArgs.Create(
					("type", GetRentalTypeLabel()),
					("value", chargedCredits)));
			return;
		}

		inventoryUiController?.PublishStatus(
			UiTextKeys.Inventory.StatusSecureInsuranceFailed,
			UiTextArgs.Create(
				("type", GetRentalTypeLabel()),
				("reason", GetInsufficientFundsReason())));
	}

	private static string BuildStatusText(RunSession? runSession)
	{
		if (runSession == null)
		{
			return UiText.Resolve(UiTextKeys.Inventory.SecureInsuranceUninitialized);
		}

		int rentalRuns = runSession.GetRemainingSecureContainerRentalRuns();
		int quota = runSession.GetRemainingSecureContainerQuota();
		int pendingBill = runSession.GetSecureContainerPendingBill();
		float greedPercent = runSession.GetSecureContainerCurrentGreedRate() * 100.0f;
		int unusedRuns = runSession.GetSecureContainerUnusedRunCount();

		if (rentalRuns > 0)
		{
			return UiText.Resolve(
				UiTextKeys.Inventory.SecureInsuranceRentalActive,
				("runs", rentalRuns),
				("bill", pendingBill),
				("greed", greedPercent),
				("unused_runs", unusedRuns),
				("quota", quota));
		}

		if (quota > 0)
		{
			return UiText.Resolve(
				UiTextKeys.Inventory.SecureInsuranceChallengeActive,
				("quota", quota),
				("bill", pendingBill),
				("greed", greedPercent),
				("unused_runs", unusedRuns));
		}

		if (pendingBill > 0)
		{
			return UiText.Resolve(
				UiTextKeys.Inventory.SecureInsuranceRetrievalOnly,
				("bill", pendingBill),
				("greed", greedPercent));
		}

		return UiText.Resolve(UiTextKeys.Inventory.SecureInsuranceNone);
	}

	private static string BuildRentalButtonText(RunSession? runSession)
	{
		bool extend = runSession?.GetRemainingSecureContainerRentalRuns() > 0;
		int rentalUses = runSession?.SecureContainerRentalUsesPerPurchase ?? 0;
		return UiText.Resolve(
			extend ? UiTextKeys.Inventory.SecureRentalExtendButton : UiTextKeys.Inventory.SecureRentalButton,
			("runs", rentalUses),
			("cost", RunSession.SecureContainerRentalCost));
	}

	private static string GetRentalTypeLabel()
	{
		return UiText.Resolve(UiTextKeys.Inventory.SecureRentalType);
	}

	private static string GetInsufficientFundsReason()
	{
		return UiText.Resolve(UiTextKeys.Inventory.InsufficientCredits);
	}
}
