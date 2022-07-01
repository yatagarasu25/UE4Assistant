namespace UE4Assistant;

public enum UModuleType
{
	Runtime,
	RuntimeNoCommandlet,
	RuntimeAndProgram,
	CookedOnly,
	Developer,
	UncookedOnly,
	DeveloperTool,
	Editor,
	EditorNoCommandlet,
	Program,
	ServerOnly,
	ClientOnly,
}

public enum UModuleLoadingPhase
{
	EarliestPossible,
	PostConfigInit,
	PreEarlyLoadingScreen,
	PreLoadingScreen,
	PreDefault,
	Default,
	PostDefault,
	PostEngineInit,
	None,
}

public class UModuleItem
{
	public string Name;
	[JsonConverter(typeof(StringEnumConverter))]
	public UModuleType Type = UModuleType.Runtime;
	[JsonConverter(typeof(StringEnumConverter))]
	public UModuleLoadingPhase LoadingPhase = UModuleLoadingPhase.Default;
}
