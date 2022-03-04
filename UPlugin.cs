using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;



namespace UE4Assistant
{
	public class UPlugin
	{
		[NonSerialized]
		public string RootPath;

		public int FileVersion = 3;
		public int Version = 1;
		public string VersionName = "0.1";
		public string FriendlyName = "";
		public string Description = "";
		public string Category = "Other";
		public string CreatedBy = "";
		public string CreatedByURL = "";
		public string DocsURL = "";
		public string MarketplaceURL = "";
		public string SupportURL = "";
		public IList<UModuleItem> Modules = new List<UModuleItem>();
		public bool EnabledByDefault = true;
		public bool CanContainContent = true;
		public bool IsBetaVersion = false;
		public bool Installed = false;


		public UPlugin()
		{
			RootPath = System.IO.Directory.GetCurrentDirectory();
		}

		static public UPlugin Load(string filename)
		{
			UPlugin plugin = JsonConvert.DeserializeObject<UPlugin>(File.ReadAllText(filename));
			plugin.RootPath = Path.GetDirectoryName(filename);
			return plugin;
		}

		public void Save(string filename, JsonIndentation jsonIndentation)
		{
			File.WriteAllText(filename, this.SerializeObject(Formatting.Indented, jsonIndentation));
		}
	}
}
