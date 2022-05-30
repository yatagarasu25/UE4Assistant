using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using SystemEx;



namespace UE4Assistant
{
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
	public class UnrealEngineInstance
	{
		public readonly string Uuid;
		public readonly string RootPath;
		public readonly UnrealEngineBuildType BuildType;


		public string EnginePath => Path.Combine(RootPath, "Engine");
		public string BinariesPath => Path.Combine(EnginePath, "Binaries");
		public string BinariesDotNETPath => Path.Combine(BinariesPath, "DotNET");
		public string BuildPath => Path.Combine(EnginePath, "Build");


		public string BaseCommitFile => Path.Combine(RootPath, ".ue.basecommit");
		public string DependenciesFile => Path.Combine(RootPath, ".ue4dependencies");
		public string SetupFile => Path.Combine(RootPath, $"Setup{Utilities.ScriptExtension}");
		public string GenerateProjectFiles => Path.Combine(BuildPath, "BatchFiles", "Mac", $"GenerateProjectFiles{Utilities.ScriptExtension}");
		public string RunUATPath => Path.Combine(BuildPath, "BatchFiles", $"RunUAT{Utilities.ScriptExtension}");
		public string UnrealEditorPath {
			get {
				var UE4 = Path.Combine(BinariesPath, "Win64", "UE4Editor.exe");
				var UE5 = Path.Combine(BinariesPath, "Win64", "UnrealEditor.exe");
				return File.Exists(UE4) ? UE4 : UE5;
			}
		}


		public UnrealEngineInstance(UnrealItemDescription unrealItem)
		{
			if (unrealItem.Type != UnrealItemType.Project)
			{
				unrealItem = UnrealItemDescription.RequireUnrealItem(unrealItem.RootPath, UnrealItemType.Project);
			}

			var availableBuilds = FindAvailableBuilds();
			var Configuration = unrealItem.ReadConfiguration<ProjectConfiguration>();
			if (!(Configuration?.UE4RootPath).IsNullOrWhiteSpace())
			{
				RootPath = Utilities.GetFullPath(Configuration.UE4RootPath, unrealItem.RootPath);
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

		public UnrealEngineInstance(string rootPath)
		{
			var availableBuilds = FindAvailableBuilds();

			RootPath = Path.GetFullPath(rootPath);
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
		}

		public void Setup()
		{
			if (BuildType != UnrealEngineBuildType.Source)
				return;

			if (!File.Exists(BaseCommitFile))
				return;

			if (!File.Exists(DependenciesFile)
				|| (File.GetLastWriteTime(BaseCommitFile) > File.GetLastWriteTime(DependenciesFile)))
			{
				Utilities.RequireExecuteCommandLine(SetupFile);
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
}
