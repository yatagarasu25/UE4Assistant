using System;
using System.Collections.Generic;
using System.IO;
using SystemEx;



namespace UE4Assistant
{
	public class UnrealEngineInstance
	{
		public readonly string RootPath;
		public readonly string RunUATPath;
		public readonly string UE4EditorPath;

		public UnrealEngineInstance(UnrealItemDescription unrealItem)
		{
			if (unrealItem.Type != UnrealItemType.Project)
			{
				unrealItem = UnrealItemDescription.DetectUnrealProject(unrealItem.FullPath);
			}

			UProject project = UProject.Load(unrealItem.FullPath);

			Dictionary<string, string> availableBuilds = new Dictionary<string, string>();

			var LocalUnrealEngine = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\EpicGames\Unreal Engine");
			var UserUnrealEngineBuilds = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Epic Games\Unreal Engine\Builds");

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

			if (!availableBuilds.TryGetValue(project.EngineAssociation, out RootPath))
			{
				throw new ArgumentException("Engine root for {0} not found in registry.".format(project.EngineAssociation));
			}

			RootPath = Path.GetFullPath(RootPath);
			RunUATPath = Path.Combine(RootPath, "Engine\\Build\\BatchFiles\\RunUAT.bat");
			UE4EditorPath = Path.Combine(RootPath, "Engine\\Binaries\\Win64\\UE4Editor.exe");
		}

		public UnrealEngineInstance(string rootPath)
		{
			if (!Directory.Exists(rootPath))
			{
				throw new ArgumentException("Engine root for {0} does not exist.".format(rootPath));
			}

			RootPath = Path.GetFullPath(rootPath);
			RunUATPath = Path.Combine(RootPath, "Engine\\Build\\BatchFiles\\RunUAT.bat");
			UE4EditorPath = Path.Combine(RootPath, "Engine\\Binaries\\Win64\\UE4Editor.exe");
		}

		public static string GetUEVersionSelectorPath()
		{
			var LocalUnrealEngine = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(@"Unreal.ProjectFile\DefaultIcon");
			if (LocalUnrealEngine != null)
			{
				return ((string)LocalUnrealEngine.GetValue("")).Trim('"', ' ');
			}

			return string.Empty;
		}
	}
}
