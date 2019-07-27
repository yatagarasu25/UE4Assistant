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
		public readonly string RootPath;
		public readonly string GenerateProjectFiles;
		public readonly string RunUATPath;
		public readonly string UE4EditorPath;

		public UnrealEngineInstance(UnrealItemDescription unrealItem)
		{
			if (unrealItem.Type != UnrealItemType.Project)
			{
				unrealItem = UnrealItemDescription.DetectUnrealItem(unrealItem.FullPath, UnrealItemType.Project);
			}

			UProject project = UProject.Load(unrealItem.FullPath);

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

			if (!availableBuilds.TryGetValue(project.EngineAssociation, out RootPath))
			{
				throw new ArgumentException("Engine root for {0} not found in registry.".format(project.EngineAssociation));
			}

			RootPath = Path.GetFullPath(RootPath);

			string EnginePath = Path.Combine(RootPath, "Engine");
			string BinariesPath = Path.Combine(EnginePath, "Binaries");
			string BuildPath = Path.Combine(EnginePath, "Build");
			GenerateProjectFiles = Path.Combine(BuildPath, "BatchFiles", "Mac", "GenerateProjectFiles.sh");
			RunUATPath = Path.Combine(BuildPath, "BatchFiles", "RunUAT.bat");
			UE4EditorPath = Path.Combine(BinariesPath, "Win64", "UE4Editor.exe");
		}

		public UnrealEngineInstance(string rootPath)
		{
			if (!Directory.Exists(rootPath))
			{
				throw new ArgumentException("Engine root for {0} does not exist.".format(rootPath));
			}

			string EnginePath = Path.Combine(RootPath, "Engine");
			string BinariesPath = Path.Combine(EnginePath, "Binaries");
			string BuildPath = Path.Combine(EnginePath, "Build");
			GenerateProjectFiles = Path.Combine(BuildPath, "BatchFiles", "Mac", "GenerateProjectFiles.sh");
			RunUATPath = Path.Combine(BuildPath, "BatchFiles", "RunUAT.bat");
			UE4EditorPath = Path.Combine(BinariesPath, "Win64", "UE4Editor.exe");
		}

		public static string GetUEVersionSelectorPath()
		{
			var LocalUnrealEngine =
				Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.ClassesRoot, Microsoft.Win32.RegistryView.Registry64)
				?.OpenSubKey(@"Unreal.ProjectFile\DefaultIcon");
			if (LocalUnrealEngine != null)
			{
				return ((string)LocalUnrealEngine.GetValue("")).Trim('"', ' ');
			}

			return string.Empty;
		}
	}
}
