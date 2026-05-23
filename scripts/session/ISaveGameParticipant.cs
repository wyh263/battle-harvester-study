using System.Text.Json;

namespace BattleHarvesterStudy.Session;

public interface ISaveGameParticipant
{
	string SaveSectionId { get; }

	JsonElement CaptureSaveSection();

	void RestoreSaveSection(JsonElement sectionData);
}
