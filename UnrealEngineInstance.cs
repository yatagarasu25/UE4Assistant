using System.Runtime.InteropServices;

namespace UE4Assistant;

public class LauncherInstallationItem
{
	public string InstallLocation;
	public string AppName;
	public string AppVersion;
}

public class LauncherInstalled
{
	public List<LauncherInstallationItem> InstallationList;
}

public enum UnrealEngineBuildType
{
	Installed,
	Source
}

public record class Version(int MajorVersion
	, int MinorVersion
	, int PatchVersion
	, int Changelist
	, int CompatibleChangelist
	, int IsLicenseeVersion
	, int IsPromotedBuild
	, string BranchName);

public class UnrealEngineInstance
{
	public readonly string Uuid;
	public readonly string RootPath;
	public readonly UnrealEngineBuildType BuildType;

	public readonly Version Version;

	public string Platform => "Unknown".Let(_ => {
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			return "Win64";
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			return "Linux";
		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			return "Mac";

		return _;
	});

	public string EnginePath => Path.Combine(RootPath, "Engine");
	public string BinariesPath => Path.Combine(EnginePath, "Binaries");
	public string BinariesDotNETPath => Path.Combine(BinariesPath, "DotNET");
	public string BuildPath => Path.Combine(EnginePath, "Build");
	public string BuildBatchFilesPath => Path.Combine(BuildPath, "BatchFiles");
	public string BuildBatchPlatformFilesPath => BuildBatchFilesPath.Let(_ => {
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			return _;
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			return Path.Combine(_, "Linux");
		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			return Path.Combine(_, "Mac");

		return _;
	});

	public string BaseCommitFile => Path.Combine(RootPath, ".ue.basecommit");
	public string NeedSetupFile => Path.Combine(RootPath, ".ue.needsetup");
	public string DependenciesFile => Path.Combine(RootPath, ".ue4dependencies");
	public string SetupFile => Path.Combine(RootPath, $"Setup{Utilities.ScriptExtension}");

	public string BuildSh => Path.Combine(BuildBatchPlatformFilesPath, $"Build{Utilities.ScriptExtension}");
	public string GenerateProjectFiles => Path.Combine(BuildBatchPlatformFilesPath, $"GenerateProjectFiles{Utilities.ScriptExtension}");
	public string RunUATSh => Path.Combine(BuildBatchFilesPath, $"RunUAT{Utilities.ScriptExtension}");
	public string RunUBTSh => Path.Combine(BuildBatchFilesPath, $"RunUBT{Utilities.ScriptExtension}");

	public string EditorBuildTarget => Version.MajorVersion == 4
			? "UE4Editor"
			: "UnrealEditor";

	public string UnrealCmdPath => Path.Combine(BinariesPath, Platform, $"{EditorBuildTarget}-Cmd.exe");
	public string UnrealEditorPath => Path.Combine(BinariesPath, Platform, $"{EditorBuildTarget}.exe");


	public UnrealEngineInstance(UnrealItemDescription unrealItem)
	{
		BuildType = UnrealEngineBuildType.Source;

		unrealItem = UnrealItemDescription.RequireUnrealItem(unrealItem.RootPath, UnrealItemType.Project, UnrealItemType.Engine);

		if (unrealItem.Type == UnrealItemType.Project)
		{
			var availableBuilds = FindAvailableBuilds();
			var Configuration = unrealItem.SanitizeConfiguration(unrealItem.ReadConfiguration<ProjectConfiguration>());
			if (!(Configuration?.UE4RootPath).IsNullOrWhiteSpace())
			{
				RootPath = Configuration.UE4RootPath;
				Uuid = "<Engine Not Registered>";

				foreach (var (uuid, path) in availableBuilds)
				{
					if (Path.GetFullPath(path.Item1) == RootPath)
					{
						Uuid = uuid;
						BuildType = path.Item2;
						break;
					}
				}
			}
			else
			{
				UProject project = UProject.Load(unrealItem.FullPath);

				if (!availableBuilds.TryGetValue(project.EngineAssociation, out var item))
				{
					throw new UEIdNotFound(project.EngineAssociation);
				}

				Uuid = project.EngineAssociation;
				RootPath = Path.GetFullPath(item.Item1);
				BuildType = item.Item2;
			}
		}
		else if (unrealItem.Type == UnrealItemType.Engine)
		{
			RootPath = Path.GetFullPath(unrealItem.RootPath);

			var availableBuilds = FindAvailableBuilds();
			foreach (var pair in availableBuilds)
			{
				if (RootPath.StartsWith(Path.GetFullPath(pair.Value.Item1)))
				{
					RootPath = pair.Value.Item1;
					BuildType = pair.Value.Item2;
					Uuid = pair.Key;

					break;
				}
			}
		}

		Version = JsonConvert.DeserializeObject<Version>(
			File.ReadAllText(Path.Combine(BuildPath, "Build.version")));
	}

	public UnrealEngineInstance(string rootPath)
	{
		RootPath = Path.GetFullPath(rootPath);
		BuildType = UnrealEngineBuildType.Source;

		var availableBuilds = FindAvailableBuilds();
		foreach (var pair in availableBuilds)
		{
			if (RootPath.StartsWith(Path.GetFullPath(pair.Value.Item1)))
			{
				RootPath = pair.Value.Item1;
				BuildType = pair.Value.Item2;
				Uuid = pair.Key;

				break;
			}
		}

		if (!Directory.Exists(RootPath))
		{
			throw new UERootNotFound(rootPath);
		}

		Version = JsonConvert.DeserializeObject<Version>(
			File.ReadAllText(Path.Combine(BuildPath, "Build.version")));
	}

	public void Setup()
	{
		if (BuildType != UnrealEngineBuildType.Source)
			return;

		if (File.Exists(BaseCommitFile))
		{
			if (!File.Exists(DependenciesFile)
				|| (File.GetLastWriteTime(BaseCommitFile) > File.GetLastWriteTime(DependenciesFile)))
			{
				Utilities.RequireExecuteCommandLine(SetupFile);
			}
		}

		if (File.Exists(NeedSetupFile))
		{
			Utilities.RequireExecuteCommandLine(SetupFile);
			File.Delete(NeedSetupFile);
		}
	}

	public static string GetUEVersionSelectorPath()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			var LocalUnrealEngine =
				Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.ClassesRoot, Microsoft.Win32.RegistryView.Registry64)
				?.OpenSubKey(@"Unreal.ProjectFile\DefaultIcon");
			if (LocalUnrealEngine != null)
			{
				return ((string)LocalUnrealEngine.GetValue("")).Trim('"', ' ');
			}
		}

		return string.Empty;
	}

	public static Dictionary<string, (string, UnrealEngineBuildType)> FindAvailableBuilds()
	{
		var availableBuilds = new Dictionary<string, (string, UnrealEngineBuildType)>();

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			var LocalUnrealEngine = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Registry64)
				?.OpenSubKey(@"SOFTWARE\EpicGames\Unreal Engine");
			var UserUnrealEngineBuilds = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentUser, Microsoft.Win32.RegistryView.Registry64)
				?.OpenSubKey(@"SOFTWARE\Epic Games\Unreal Engine\Builds");

			if (LocalUnrealEngine != null)
			{
				foreach (string build in LocalUnrealEngine.GetSubKeyNames())
				{
					string ueroot = Path.GetFullPath((string)LocalUnrealEngine.OpenSubKey(build).GetValue("InstalledDirectory"));

					if (!string.IsNullOrWhiteSpace(ueroot))
					{
						availableBuilds.Add(build, (ueroot, UnrealEngineBuildType.Installed));
					}
				}
			}

			if (UserUnrealEngineBuilds != null)
			{
				foreach (string build in UserUnrealEngineBuilds.GetValueNames())
				{
					string ueroot = Path.GetFullPath((string)UserUnrealEngineBuilds.GetValue(build));

					if (!string.IsNullOrWhiteSpace(ueroot))
					{
						try
						{
							availableBuilds.Add(new Guid(build).ToString("B").ToUpper(), (ueroot, UnrealEngineBuildType.Source));
						}
						catch
						{
							availableBuilds.Add(build, (ueroot, UnrealEngineBuildType.Source));
						}
					}
				}
			}
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			string EpicAppSupportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Epic");
			string LauncherInstalledPath = Path.Combine(EpicAppSupportPath, "UnrealEngineLauncher", "LauncherInstalled.dat");

			try
			{
				LauncherInstalled Installed = JsonConvert.DeserializeObject<LauncherInstalled>(File.ReadAllText(LauncherInstalledPath));
				foreach (LauncherInstallationItem Item in Installed.InstallationList)
				{
					availableBuilds.Add(Item.AppName.Replace("UE_", ""), (Item.InstallLocation, UnrealEngineBuildType.Installed));
				}
			}
			catch { }
		}

		return availableBuilds;
	}

	public static void SetUserUnrealEngineBuilds(string uuid, string path)
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			var UserUnrealEngineBuilds = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentUser, Microsoft.Win32.RegistryView.Registry64)
				?.OpenSubKey(@"SOFTWARE\Epic Games\Unreal Engine\Builds", Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree);

			UserUnrealEngineBuilds ??= Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentUser, Microsoft.Win32.RegistryView.Registry64)
				?.CreateSubKey(@"SOFTWARE\Epic Games\Unreal Engine\Builds", Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree);

			if (UserUnrealEngineBuilds != null)
			{
				string foundUuid = null;
				foreach (string build in UserUnrealEngineBuilds.GetValueNames())
				{
					string ueroot = Path.GetFullPath((string)UserUnrealEngineBuilds.GetValue(build));

					if (ueroot == path)
					{
						foundUuid = build;
						break;
					}
					if (build == uuid)
					{
						foundUuid = build;
						break;
					}
				}

				if (foundUuid != null)
				{
					UserUnrealEngineBuilds.DeleteValue(foundUuid);
				}
				UserUnrealEngineBuilds.SetValue(uuid, path.Replace('\\', '/'));
			}
		}
	}
}
