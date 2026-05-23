# Save System Design

The save file uses a sectioned JSON format:

```json
{
  "version": 1,
  "sections": {
    "run_session_core": {},
    "future_system_id": {}
  }
}
```

`run_session_core` is owned by `RunSession` and stores the current player/session state:

- player inventory
- equipment
- installed weapon skill state
- warehouse
- secure containers
- credits
- run/extraction flags
- secure container billing state

New saveable systems should not add fields to `RunSession.SaveGameData` unless the data is truly part of the core run session. Instead, create a node that implements `ISaveGameParticipant`.

## Adding A Save Section

1. Implement `ISaveGameParticipant`.
2. Return a stable `SaveSectionId`, for example `faction_reputation` or `quest_log`.
3. Add the node to `RunSession.SaveGameParticipantGroupName` in `_Ready`.
4. Remove it from the group in `_ExitTree` if needed.
5. Call `RunSession.Instance?.SaveGameToDisk()` when that system changes.

Example:

```csharp
public partial class QuestLog : Node, ISaveGameParticipant
{
	public string SaveSectionId => "quest_log";

	public override void _Ready()
	{
		AddToGroup(RunSession.SaveGameParticipantGroupName);
		RunSession.Instance?.RestoreRegisteredSaveParticipants();
	}

	public JsonElement CaptureSaveSection()
	{
		return JsonSerializer.SerializeToElement(new QuestLogSaveData());
	}

	public void RestoreSaveSection(JsonElement sectionData)
	{
		QuestLogSaveData? data = sectionData.Deserialize<QuestLogSaveData>();
		if (data == null)
		{
			return;
		}

		// Restore local state.
	}
}
```

This keeps save ownership close to the system that owns the data. The central save file writer only collects named sections and writes them to disk.
