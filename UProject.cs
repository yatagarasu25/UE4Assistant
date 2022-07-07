namespace UE4Assistant;

public class UProject
{
	[NonSerialized]
	public string RootPath;
	[NonSerialized]
	public string Name;

	public int FileVersion = 3;
	public string EngineAssociation = string.Empty;
	public string Category = string.Empty;
	public string Description = string.Empty;
	public List<UModuleItem> Modules = new();
	public List<UPluginItem> Plugins = new();
	public List<string> TargetPlatforms = new();

	public bool ShouldSerializeModules() => Modules != null && Modules.Count > 0;
	public bool ShouldSerializePlugins() => Plugins != null && Plugins.Count > 0;
	public bool ShouldSerializeTargetPlatforms() => TargetPlatforms != null && TargetPlatforms.Count > 0;

	public UProject(string name)
	{
		RootPath = System.IO.Directory.GetCurrentDirectory();
		Name = name;
	}

	static public UProject Load(string filename)
	{
		UProject project = JsonConvert.DeserializeObject<UProject>(File.ReadAllText(filename));
		project.RootPath = Path.GetDirectoryName(filename);
		project.Name = Path.GetFileNameWithoutExtension(filename);
		return project;
	}

	public void Save(JsonIndentation jsonIndentation) => Save(Path.Combine(RootPath, Name + ".uproject"), jsonIndentation);
	public void Save(string filename, JsonIndentation jsonIndentation)
	{
		File.WriteAllText(filename, this.SerializeObject(Formatting.Indented, jsonIndentation));
	}
}
