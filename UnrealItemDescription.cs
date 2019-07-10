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

		public static UnrealItemDescription DetectUnrealItem(string path)
		{
			foreach (string file in Directory.GetFiles(path))
			{
				if (file.EndsWith(".uproject"))
					return new UnrealItemDescription(UnrealItemType.Project, file);
				else if (file.EndsWith(".uplugin"))
					return new UnrealItemDescription(UnrealItemType.Plugin, file);
				else if (file.EndsWith(".Build.cs"))
					return new UnrealItemDescription(UnrealItemType.Module, file);
			}

			string basePath = Path.GetFullPath(Path.Combine(path, ".."));
			if (basePath != path)
				return DetectUnrealItem(basePath);

			return null;
		}

		public static UnrealItemDescription DetectUnrealProject(string path, string projectName = null)
		{
			foreach (string file in Directory.GetFiles(path))
			{
				if (file.EndsWith((string.IsNullOrWhiteSpace(projectName) ? "" : projectName) + ".uproject"))
					return new UnrealItemDescription(UnrealItemType.Project, file);
				//else if (file.EndsWith((string.IsNullOrWhiteSpace(projectName) ? "" : projectName) + ".uplugin"))
				//	return new UnrealItemDescription { Type = UnrealItemType.Plugin, RootPath = path, ItemName = Path.GetFileName(file) };
			}

			string basePath = Path.GetFullPath(Path.Combine(path, ".."));
			if (basePath != path)
				return DetectUnrealProject(basePath);

			return null;
		}

		public static UnrealItemDescription DetectUnrealModule(string path)
		{
			foreach (string file in Directory.GetFiles(path))
			{
				if (file.EndsWith(".Build.cs"))
					return new UnrealItemDescription(UnrealItemType.Module, file);
			}

			string basePath = Path.GetFullPath(Path.Combine(path, ".."));
			if (basePath != path)
				return DetectUnrealModule(basePath);

			return null;
		}
	}
}