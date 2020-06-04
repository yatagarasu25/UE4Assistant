using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;



namespace UE4Assistant
{
	public enum UnrealItemType
	{
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

		public string ProjectLogPath => Path.Combine(RootPath, "Saved", "Logs", Name + ".log");

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
				if (file.EndsWith(postfix + ".uproject") && Array.IndexOf(types, UnrealItemType.Project) != -1)
					return new UnrealItemDescription(UnrealItemType.Project, file);
				else if (file.EndsWith(postfix + ".uplugin") && Array.IndexOf(types, UnrealItemType.Plugin) != -1)
					return new UnrealItemDescription(UnrealItemType.Plugin, file);
				else if (file.EndsWith(postfix + ".Build.cs") && Array.IndexOf(types, UnrealItemType.Module) != -1)
					return new UnrealItemDescription(UnrealItemType.Module, file);
			}

			string basePath = Path.GetFullPath(Path.Combine(path, ".."));
			if (basePath != path)
				return DetectUnrealItem(basePath, types);

			return null;
		}

		public static UnrealItemDescription DetectUnrealItem(string path, params UnrealItemType[] types)
		{
			return DetectUnrealItem(path, "", types);
		}

		public static UnrealItemDescription DetectUnrealItem(string path)
		{
			return DetectUnrealItem(path, UnrealItemType.Project, UnrealItemType.Plugin, UnrealItemType.Module);
		}
	}
}