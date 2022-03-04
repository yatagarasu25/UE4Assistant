using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;



namespace UE4Assistant
{
	public class UProject
	{
		[NonSerialized]
		public string RootPath;
		[NonSerialized]
		public string Name;

		public int FileVersion = 3;
		public string EngineAssociation = "";
		public string Category = "";
		public string Description = "";
		public List<UModuleItem> Modules = new List<UModuleItem>();
		public List<UPluginItem> Plugins = new List<UPluginItem>();
		public List<string> TargetPlatforms = new List<string>();

		public bool ShouldSerializeModules() => Modules != null && Modules.Count > 0;
		public bool ShouldSerializePlugins() => Plugins != null && Plugins.Count > 0;
		public bool ShouldSerializeTargetPlatforms() => TargetPlatforms != null && TargetPlatforms.Count > 0;

		public UProject()
		{
			RootPath = System.IO.Directory.GetCurrentDirectory();
		}

		static public UProject Load(string filename)
		{
			UProject project = JsonConvert.DeserializeObject<UProject>(File.ReadAllText(filename));
			project.RootPath = Path.GetDirectoryName(filename);
			return project;
		}

		public void Save(string filename, JsonIndentation jsonIndentation)
		{
			File.WriteAllText(filename, this.SerializeObject(Formatting.Indented, jsonIndentation));
		}
	}
}
