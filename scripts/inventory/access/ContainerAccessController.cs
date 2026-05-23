using Godot;
using Godot.Collections;
using BattleHarvesterStudy.Presentation;

namespace BattleHarvesterStudy.Inventory;

public partial class ContainerAccessController : Node
{
	[Signal]
	public delegate void ContainerUnlockedEventHandler();

	[Signal]
	public delegate void ContainerAccessConsumedEventHandler();

	[Export]
	public bool LockedByDefault { get; set; }

	[Export]
	public bool EnforceDistanceCheck { get; set; } = true;

	[Export(PropertyHint.Range, "0,100,0.1")]
	public float RequiredDistance { get; set; } = 2.5f;

	[Export]
	public Array<string> RequiredAccessTags { get; set; } = [];

	[Export]
	public bool SingleUseAccess { get; set; }

	public bool IsLocked { get; private set; }
	public bool HasBeenAccessed { get; private set; }

	private Node3D? _owner;

	public override void _Ready()
	{
		_owner = GetOwner<Node3D>();
		IsLocked = LockedByDefault;
	}

	public void Unlock()
	{
		if (!IsLocked)
		{
			return;
		}

		IsLocked = false;
		EmitSignal(SignalName.ContainerUnlocked);
	}

	public void Lock()
	{
		IsLocked = true;
	}

	public void ResetAccessState(bool keepLockState = true)
	{
		HasBeenAccessed = false;
		if (!keepLockState)
		{
			IsLocked = LockedByDefault;
		}
	}

	public ContainerAccessCheckResult CheckAccess(Node3D? requester)
	{
		if (requester == null || !GodotObject.IsInstanceValid(requester))
		{
			return new ContainerAccessCheckResult
			{
				CanAccess = false,
				BlockReason = ContainerAccessBlockReason.MissingRequester,
				RequiredDistance = RequiredDistance,
				CurrentDistance = float.PositiveInfinity,
				FailureTextKey = UiTextKeys.Inventory.AccessMissingRequester,
				FailureFormatArgs = UiTextArgs.Create(),
			};
		}

		if (IsLocked)
		{
			return new ContainerAccessCheckResult
			{
				CanAccess = false,
				BlockReason = ContainerAccessBlockReason.Locked,
				RequiredDistance = RequiredDistance,
				CurrentDistance = GetDistanceToRequester(requester),
				FailureTextKey = UiTextKeys.Inventory.AccessLocked,
				FailureFormatArgs = UiTextArgs.Create(),
			};
		}

		if (SingleUseAccess && HasBeenAccessed)
		{
			return new ContainerAccessCheckResult
			{
				CanAccess = false,
				BlockReason = ContainerAccessBlockReason.SingleUseConsumed,
				RequiredDistance = RequiredDistance,
				CurrentDistance = GetDistanceToRequester(requester),
				FailureTextKey = UiTextKeys.Inventory.AccessSingleUseConsumed,
				FailureFormatArgs = UiTextArgs.Create(),
			};
		}

		float distance = GetDistanceToRequester(requester);
		if (EnforceDistanceCheck && distance > RequiredDistance)
		{
			return new ContainerAccessCheckResult
			{
				CanAccess = false,
				BlockReason = ContainerAccessBlockReason.OutOfRange,
				RequiredDistance = RequiredDistance,
				CurrentDistance = distance,
				FailureTextKey = UiTextKeys.Inventory.AccessOutOfRange,
				FailureFormatArgs = UiTextArgs.Create(
					("current", distance.ToString("0.0")),
					("required", RequiredDistance.ToString("0.0"))),
			};
		}

		if (!HasRequiredTags(requester, out string missingTag))
		{
			return new ContainerAccessCheckResult
			{
				CanAccess = false,
				BlockReason = ContainerAccessBlockReason.MissingRequiredTag,
				RequiredDistance = RequiredDistance,
				CurrentDistance = distance,
				FailureTextKey = UiTextKeys.Inventory.AccessMissingTag,
				FailureFormatArgs = UiTextArgs.Create(("tag", missingTag)),
			};
		}

		return new ContainerAccessCheckResult
		{
			CanAccess = true,
			BlockReason = ContainerAccessBlockReason.None,
			RequiredDistance = RequiredDistance,
			CurrentDistance = distance,
			FailureTextKey = string.Empty,
			FailureFormatArgs = UiTextArgs.Create(),
		};
	}

	public bool TryConsumeAccess(Node3D? requester, out ContainerAccessCheckResult result)
	{
		result = CheckAccess(requester);
		if (!result.CanAccess)
		{
			return false;
		}

		if (SingleUseAccess)
		{
			HasBeenAccessed = true;
			EmitSignal(SignalName.ContainerAccessConsumed);
		}

		return true;
	}

	private float GetDistanceToRequester(Node3D requester)
	{
		if (_owner == null)
		{
			return 0.0f;
		}

		return _owner.GlobalPosition.DistanceTo(requester.GlobalPosition);
	}

	private bool HasRequiredTags(Node requester, out string missingTag)
	{
		missingTag = string.Empty;
		foreach (string tag in RequiredAccessTags)
		{
			if (string.IsNullOrWhiteSpace(tag))
			{
				continue;
			}

			if (requester.IsInGroup(GetAccessTagGroupName(tag)) || requester.IsInGroup(tag))
			{
				continue;
			}

			missingTag = tag;
			return false;
		}

		return true;
	}

	public static string GetAccessTagGroupName(string tag)
	{
		return $"access_tag:{tag}";
	}
}
