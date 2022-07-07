namespace UE4Assistant;

public class UPlugin
{
	[NonSerialized]
	public string RootPath;
	[NonSerialized]
	public string Name;

	public int FileVersion = 3;
	public int Version = 1;
	public string VersionName = "0.1";
	public string FriendlyName = string.Empty;
	public string Description = string.Empty;
	public string Category = "Other";
	public string CreatedBy = string.Empty;
	public string CreatedByURL = string.Empty;
	public string DocsURL = string.Empty;
	public string MarketplaceURL = string.Empty;
	public string SupportURL = string.Empty;
	public List<UModuleItem> Modules = new();
	public bool EnabledByDefault = true;
	public bool CanContainContent = true;
	public bool IsBetaVersion = false;
	public bool Installed = false;


	public UPlugin(string name)
	{
		RootPath = Directory.GetCurrentDirectory();
		Name = name;
	}

	static public UPlugin Load(string filename)
	{
		UPlugin plugin = JsonConvert.DeserializeObject<UPlugin>(File.ReadAllText(filename));
		plugin.RootPath = Path.GetDirectoryName(filename);
		plugin.Name = Path.GetFileNameWithoutExtension(filename);
		return plugin;
	}


	public void Save(JsonIndentation jsonIndentation) => Save(Path.Combine(RootPath, Name + ".uplugin"), jsonIndentation);
	public void Save(string filename, JsonIndentation jsonIndentation)
	{
		File.WriteAllText(filename, this.SerializeObject(Formatting.Indented, jsonIndentation));
	}
}
