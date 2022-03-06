using System.Runtime.InteropServices;

namespace UE4Assistant
{
	public class UnrealCookSettings
	{
		public string UE4RootPath = null;
		public string ProjectFullPath = null;
		public string Target = null;
		public string Platform = null;
		public string CookFlavor = null;
		public string ClientConfig;
		public string ServerConfig;
		public bool? UseP4 = null;
		public bool? Cook = null;
		public bool? AllMaps = null;
		public bool? Client = null;
		public bool? Server = null;
		public bool? Build = null;
		public bool? Stage = null;
		public bool? Pak = null;
		public bool? Archive = null;
		public bool? Package = null;
		public bool? Compressed = null;
		public bool? NoXGE = null;
		public bool? NoFASTBuild = null;
		public string ArchiveDirectory;

		public static string DefaultPlatformName =>
			RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Win64"
			: RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux"
			: RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "Mac"
			: "Unknown";

		public static UnrealCookSettings CreateDefaultSettings() =>
			new UnrealCookSettings {
				UE4RootPath = null,
				ProjectFullPath = null,
				Target = null,
				Platform = DefaultPlatformName,
				ClientConfig = "Development",
				ServerConfig = "Development",
				UseP4 = false,
				Cook = true,
				AllMaps = true,
				Server = false,
				Build = true,
				Stage = true,
				Pak = true,
				Archive = true,
				Package = true,
				ArchiveDirectory = "./Packages",
			};

		public static UnrealCookSettings CreateBuildSettings()
			=> new UnrealCookSettings {
				UE4RootPath = null,
				ProjectFullPath = null,
				Target = null,
				Platform = DefaultPlatformName,
				ClientConfig = "Development",
				ServerConfig = "Development",
				UseP4 = false,
				AllMaps = true,
				Server = false,
				Build = true,
				ArchiveDirectory = null,
			};

		public override string ToString()
		{
			return (!string.IsNullOrWhiteSpace(ProjectFullPath) ? string.Format("-project=\"{0}\"", ProjectFullPath) : "")
				+ (Platform != null ? string.Format(" -platform=\"{0}\"", Platform) : "")
				+ (Target != null ? string.Format(" -target=\"{0}\"", Target) : "")
				+ (CookFlavor != null ? string.Format(" -cookflavor=\"{0}\"", CookFlavor) : "")
				+ string.Format(" -clientconfig=\"{0}\"", ClientConfig)
				+ string.Format(" -serverconfig=\"{0}\"", ServerConfig)
				+ (UseP4 != null ? UseP4.Value ? " -P4" : " -noP4" : "")
				+ (Cook != null ? Cook.Value ? " -cook" : "" : "")
				+ (AllMaps != null ? AllMaps.Value ? " -allmaps" : "" : "")
				+ (Client != null ? Client.Value ? " -client" : " -noclient" : "")
				+ (Server != null ? Server.Value ? " -server" : " -noserver" : "")
				+ (Build != null ? Build.Value ? " -build" : "" : "")
				+ (Stage != null ? Stage.Value ? " -stage" : "" : "")
				+ (Pak != null ? Pak.Value ? " -pak" : "" : "")
				+ (Archive != null ? Archive.Value ? " -archive" : "" : "")
				+ (Package != null ? Package.Value ? " -package" : "" : "")
				+ (Compressed != null ? Compressed.Value ? " -compressed" : "" : "")
				+ (NoXGE != null ? NoXGE.Value ? " -NoXGE" : "" : "")
				+ (NoFASTBuild != null ? NoFASTBuild.Value ? " -NoFASTBuild" : "" : "")
				+ (!string.IsNullOrWhiteSpace(ArchiveDirectory) ? string.Format(" -archivedirectory=\"{0}\"", ArchiveDirectory) : "")
				;
		}
	}
}
