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
		public IList<UModule> Modules = new List<UModule>();



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

		public void Save(string filename)
		{
			File.WriteAllText(filename, JsonConvert.SerializeObject(this, Formatting.Indented));
		}
	}
}
