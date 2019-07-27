namespace UE4Assistant
{
	public enum UModuleType
	{
		Runtime,
		RuntimeNoCommandlet,
		RuntimeAndProgram,
		CookedOnly,
		Developer,
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

	public class UModule
	{
		public string Name;
		public UModuleType Type = UModuleType.Runtime;
		public UModuleLoadingPhase LoadingPhase = UModuleLoadingPhase.Default;
	}
}
