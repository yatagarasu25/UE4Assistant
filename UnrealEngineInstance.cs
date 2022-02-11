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

	public class UnrealEngineInstance
	{
		public readonly string Uuid;
		public readonly string RootPath;


		public string EnginePath => Path.Combine(RootPath, "Engine");
		public string BinariesPath => Path.Combine(EnginePath, "Binaries");
		public string BinariesDotNETPath => Path.Combine(BinariesPath, "DotNET");
		public string BuildPath => Path.Combine(EnginePath, "Build");


		public string GenerateProjectFiles => Path.Combine(BuildPath, "BatchFiles", "Mac", "GenerateProjectFiles.sh");
		public string RunUATPath => Path.Combine(BuildPath, "BatchFiles", RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "RunUAT.bat" : "RunUAT.sh");
		public string UE4EditorPath => Path.Combine(BinariesPath, "Win64", "UE4Editor.exe");


		public UnrealEngineInstance(UnrealItemDescription unrealItem)
		{
			if (unrealItem.Type != UnrealItemType.Project)
			{
				unrealItem = UnrealItemDescription.DetectUnrealItem(unrealItem.FullPath, UnrealItemType.Project);
			}

			if (unrealItem == null)
			{
				throw new ArgumentException($"Can not find project root.");
			}

			var Configuration = unrealItem.ReadConfiguration<ProjectConfiguration>();
			if (Configuration != null)
			{
				RootPath = Path.GetFullPath(Configuration.UE4RootPath);
			}
			else
			{
				UProject project = UProject.Load(unrealItem.FullPath);
				Dictionary<string, string> availableBuilds = FindAvailableBuilds();

				if (!availableBuilds.TryGetValue(project.EngineAssociation, out RootPath))
				{
					throw new ArgumentException($"Engine root for {project.EngineAssociation} not found in registry.");
				}

				Uuid = project.EngineAssociation;
				RootPath = Path.GetFullPath(RootPath);
			}
		}

		public UnrealEngineInstance(string rootPath)
		{
			Dictionary<string, string> availableBuilds = FindAvailableBuilds();

			RootPath = Path.GetFullPath(rootPath);
			foreach (var pair in availableBuilds)
			{
				if (RootPath.StartsWith(Path.GetFullPath(pair.Value)))
				{
					RootPath = pair.Value;
					Uuid = pair.Key;

					break;
				}
			}

			if (!Directory.Exists(RootPath))
			{
				throw new ArgumentException("Engine root for {0} does not exist.".format(rootPath));
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

		private static Dictionary<string, string> FindAvailableBuilds()
		{
			Dictionary<string, string> availableBuilds = new Dictionary<string, string>();

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
						string ueroot = (string)LocalUnrealEngine.OpenSubKey(build).GetValue("InstalledDirectory");

						if (!string.IsNullOrWhiteSpace(ueroot))
						{
							availableBuilds.Add(build, ueroot);
						}
					}
				}

				if (UserUnrealEngineBuilds != null)
				{
					foreach (string build in UserUnrealEngineBuilds.GetValueNames())
					{
						string ueroot = (string)UserUnrealEngineBuilds.GetValue(build);

						if (!string.IsNullOrWhiteSpace(ueroot))
						{
							availableBuilds.Add(build, ueroot);
						}
					}
				}
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				string EpicAppSupportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Epic");
				string LauncherInstalledPath = Path.Combine(EpicAppSupportPath, "UnrealEngineLauncher", "LauncherInstalled.dat");

				LauncherInstalled Installed = JsonConvert.DeserializeObject<LauncherInstalled>(File.ReadAllText(LauncherInstalledPath));
				foreach (LauncherInstallationItem Item in Installed.InstallationList)
				{
					availableBuilds.Add(Item.AppName.Replace("UE_", ""), Item.InstallLocation);
				}
			}

			return availableBuilds;
		}

	}
}
