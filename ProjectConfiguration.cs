namespace UE4Assistant;

public class ProjectConfiguration
{
	public struct GenerateProjectConfiguration
	{
		public bool onAddItem;
		public bool onCode;
		public bool onEditor;
		public bool onBuild;
		public bool onCook;

		public GenerateProjectConfiguration(bool v) : this()
		{
			onAddItem = v;
			onCode = v;
			onEditor = v;
			onBuild = v;
			onCook = v;
		}
	}

	public string UE4RootPath = null;
	public GenerateProjectConfiguration GenerateProject = new GenerateProjectConfiguration(false);
	public string InterfaceSuffix = "Interface";
	public string FunctionLibrarySuffix = "Statics";
	public string DefaultBuildConfigurationFile = null;
	public string DefaultCookConfigurationFile = null;
	public JsonIndentation JsonIndentation = JsonIndentation.Default;
}
