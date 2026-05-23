using Godot;
using Godot.Collections;

namespace BattleHarvesterStudy.Presentation;

public static class UiTextArgs
{
	public static Dictionary<string, Variant> Create(params (string Name, Variant Value)[] pairs)
	{
		Dictionary<string, Variant> args = [];
		foreach ((string name, Variant value) in pairs)
		{
			args[name] = value;
		}

		return args;
	}
}
