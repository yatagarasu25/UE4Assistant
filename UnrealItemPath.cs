namespace UE4Assistant;

public enum UnrealItemPathType
{
	Unknow,
	Public,
	Classes,
	Private,
	Common,
}

public class UnrealItemPath
{
	public static UnrealItemPath Empty = new UnrealItemPath();

	public bool isFile;
	public UnrealItemDescription Module;
	public UnrealItemPathType Type;
	public string FullPath;
	public string LocalPath;
	public string ItemPath;

	protected UnrealItemPath()
	{
		Type = UnrealItemPathType.Unknow;
	}

	public UnrealItemPath(UnrealItemDescription module, string path)
	{
		isFile = File.Exists(path);

		if (!path.StartsWith(module.RootPath))
		{
			Type = UnrealItemPathType.Unknow;
		}

		string localPath = path.Substring(module.RootPath.Length);

		Module = module;

		var localPathTokens = localPath.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
		LocalPath = Path.Combine(localPathTokens);
		FullPath = Path.Combine(module.RootPath, LocalPath);
		if (localPathTokens.Length > 0)
		{
			if (localPathTokens[0].ToLower() == "private")
			{
				Type = UnrealItemPathType.Private;
				ItemPath = Path.Combine(localPathTokens.Skip(1));
			}
			else if (localPathTokens[0].ToLower() == "public")
			{
				Type = UnrealItemPathType.Public;
				ItemPath = Path.Combine(localPathTokens.Skip(1));
			}
			else if (localPathTokens[0].ToLower() == "classes")
			{
				Type = UnrealItemPathType.Classes;
				ItemPath = Path.Combine(localPathTokens.Skip(1));
			}
			else
			{
				Type = UnrealItemPathType.Common;
				ItemPath = Path.Combine(localPathTokens);
			}
		}
		else
		{
			Type = UnrealItemPathType.Common;
			ItemPath = string.Empty;
		}
	}
}
