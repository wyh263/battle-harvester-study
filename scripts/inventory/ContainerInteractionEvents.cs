using System;

namespace BattleHarvesterStudy.Inventory;

public static class ContainerInteractionEvents
{
	public static event Action<ContainerInteractionEvent>? InteractionPublished;

	public static void Publish(ContainerInteractionEvent interactionEvent)
	{
		InteractionPublished?.Invoke(interactionEvent);
	}
}
