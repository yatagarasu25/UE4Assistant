using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UE4Assistant.Templates;
using UE4Assistant.Templates.Source;



namespace UE4Assistant
{
	public class UProject
	{
		protected string RootPath;

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

		public void AddModule(UModule module)
		{
			if (Modules.Where((m) => m.Name == module.Name).Any())
			{
				return;
			}

			Modules.Add(module);

			string sourcePath = Path.Combine(RootPath, "Source");
			string modulePath = Path.Combine(sourcePath, module.Name);
			string privatePath = Path.Combine(modulePath, "Private");
			string publicPath = Path.Combine(modulePath, "Public");

			Directory.CreateDirectory(sourcePath);
			Directory.CreateDirectory(modulePath);

			Dictionary<string, object> parameters = new Dictionary<string, object>
				{
					{ "modulename", module.Name },
					{ "isprimary", true },
				};

			File.WriteAllText(Path.Combine(modulePath, module.Name + ".Build.cs")
				, Template.TransformToText<ModuleBuild_cs>(parameters));
			File.WriteAllText(Path.Combine(modulePath, module.Name + "PrivatePCH.h")
				, Template.TransformToText<PrivatePCH_h>(parameters));
			File.WriteAllText(Path.Combine(modulePath, module.Name + ".cpp")
				, Template.TransformToText<Module_cpp>(parameters));
			File.WriteAllText(Path.Combine(modulePath, module.Name + ".h")
				, Template.TransformToText<Module_h>(parameters));
		}
	}
}
