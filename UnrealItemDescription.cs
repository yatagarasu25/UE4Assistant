namespace UE4Assistant;

public enum UnrealItemType
{
	Engine,
	Project,
	Plugin,
	Module,
}

public class UE4EditorModules
{
	public string BuildId = string.Empty;
	public Dictionary<string, string> Modules = new Dictionary<string, string>();
}

public class UnrealItemDescription
{
	public UnrealItemType Type;
	public string Name;

	public string RootPath;
	public string ItemFileName;
	public string FullPath => Path.Combine(RootPath, ItemFileName);
	public string ModuleApiTag => Name.ToUpper() + "_API";
	public bool HasPublicPrivateFolders => Directory.Exists(Path.Combine(RootPath, "Public")) || Directory.Exists(Path.Combine(RootPath, "Private"));
	public string ModulePublicPath => HasPublicPrivateFolders ? Path.Combine(RootPath, "Public") : RootPath;
	public string ModulePrivatePath => HasPublicPrivateFolders ? Path.Combine(RootPath, "Private") : RootPath;
	public string ModuleClassesPath {
		get {
			var classesPath = Path.Combine(RootPath, "Classes");
			if (Directory.Exists(classesPath))
				return classesPath;

			return ModulePublicPath;
		}
	}

	public UE4EditorModules BuildModules {
		get {
			var modulesPath = Path.Combine(RootPath, "Binaries", "Win64", "UE4Editor.modules");
			if (!File.Exists(modulesPath))
				return null;

			return JsonConvert.DeserializeObject<UE4EditorModules>(File.ReadAllText(modulesPath));
		}
	}

	public string BuildLogPath => Path.Combine(RootPath, "Intermediate", "Build", "Unused");
	public IEnumerable<string> BuildLogs {
		get {
			if (!Directory.Exists(BuildLogPath))
				yield break;

			foreach (var logFile in Directory.GetFiles(BuildLogPath, "*.log", SearchOption.TopDirectoryOnly))
			{
				var logName = Path.GetFileNameWithoutExtension(logFile);
				if (logName == Name)
					yield return logFile;

				if (logName == "UE4")
					yield return logFile;
			}
		}
	}

	public string ProjectLogPath => Path.Combine(RootPath, "Saved", "Logs");
	public string ProjectLogFile => Path.Combine(ProjectLogPath, Name + ".log");
	public string ConfigurationPath => Path.Combine(RootPath, ".ue4a");
	public TConfiguration ReadConfiguration<TConfiguration>()
			where TConfiguration : new()
		=> File.Exists(ConfigurationPath)
			? Configuration.ReadConfiguration<TConfiguration>(ConfigurationPath)
			: default;

	public UnrealItemDescription(UnrealItemType type, string path)
	{
		Type = type;
		RootPath = Path.GetDirectoryName(path);
		ItemFileName = Path.GetFileName(path);

		switch (type)
		{
			case UnrealItemType.Project:
				Name = Path.GetFileNameWithoutExtension(ItemFileName);
				break;
			case UnrealItemType.Plugin:
				Name = Path.GetFileNameWithoutExtension(ItemFileName);
				break;
			case UnrealItemType.Module:
				Name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(ItemFileName));
				break;
		}
	}

	public static UnrealItemDescription DetectUnrealItem(string path, string postfix, params UnrealItemType[] types)
	{
		foreach (string file in Directory.GetFiles(path))
		{
			if (types.Contains(UnrealItemType.Project) && file.EndsWith(postfix + ".uproject"))
				return new UnrealItemDescription(UnrealItemType.Project, file);
			else if (file.EndsWith(postfix + ".uplugin") && types.Contains(UnrealItemType.Plugin))
				return new UnrealItemDescription(UnrealItemType.Plugin, file);
			else if (file.EndsWith(postfix + ".Build.cs") && types.Contains(UnrealItemType.Module))
				return new UnrealItemDescription(UnrealItemType.Module, file);
			else if (types.Contains(UnrealItemType.Engine) && file.EndsWith("GenerateProjectFiles.bat"))
				return new UnrealItemDescription(UnrealItemType.Engine, file);
		}

		string basePath = Path.GetFullPath(Path.Combine(path, ".."));
		if (basePath != path)
			return DetectUnrealItem(basePath, types);

		return null;
	}

	public static UnrealItemDescription DetectUnrealItem(string path, params UnrealItemType[] types)
		=> DetectUnrealItem(path, "", types);

	public static UnrealItemDescription DetectUnrealItemExceptTemp(string path, params UnrealItemType[] types)
	{
		return !(path?.StartsWith(Path.GetDirectoryName(Path.GetTempPath())) ?? true) ? DetectUnrealItem(path, "", types) : null;
	}

	public static UnrealItemDescription DetectUnrealItem(string path)
		=> DetectUnrealItem(path, UnrealItemType.Project, UnrealItemType.Plugin, UnrealItemType.Module);

	public static UnrealItemDescription RequireUnrealItem(string path, string postfix, params UnrealItemType[] types)
		=> DetectUnrealItem(path, postfix, types) ?? throw new RequireUnrealItemException(path, types);

	public static UnrealItemDescription RequireUnrealItem(string path, params UnrealItemType[] types)
		=> DetectUnrealItem(path, "", types) ?? throw new RequireUnrealItemException(path, types);

	public ProjectConfiguration SanitizeConfiguration(ProjectConfiguration s)
	{
		if (s == null)
			return s;

		s.UE4RootPath = s.UE4RootPath?.Let(p => Utilities.GetFullPath(p, RootPath));

		return s;
	}

	public UnrealCookSettings SanitizeSettings(UnrealCookSettings s)
	{
		if (s == null)
			return s;

		s.UE4RootPath = s.UE4RootPath?.Let(p => Utilities.GetFullPath(p, RootPath));
		s.ArchiveDirectory = s.ArchiveDirectory?.Let(p => Utilities.GetFullPath(p, RootPath));

		return s;
	}
}