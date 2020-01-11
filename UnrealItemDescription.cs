using System;
using System.IO;



namespace UE4Assistant
{
	public enum UnrealItemType
	{
		Project,
		Plugin,
		Module,
	}

	public class UnrealItemDescription
	{
		public UnrealItemType Type;
		public string Name;

		public string RootPath;
		public string ItemFileName;
		public string FullPath { get { return Path.Combine(RootPath, ItemFileName); } }
		public string ModuleApiTag { get { return Name.ToUpper() + "_API"; } }
		public string ModulePublicPath { get { return Path.Combine(RootPath, "Public"); } }
		public string ModulePrivatePath { get { return Path.Combine(RootPath, "Private"); } }
		public string ModuleClassesPath {
			get {
				string classesPath = Path.Combine(RootPath, "Classes");
				if (Directory.Exists(classesPath))
					return classesPath;

				return ModulePublicPath;
			}
		}

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