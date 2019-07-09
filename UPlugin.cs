using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UE4Assistant.Templates;
using UE4Assistant.Templates.Source;



namespace UE4Assistant
{
	public class UPlugin
	{
		protected string RootPath;

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
		public IList<UModule> Modules = new List<UModule>();
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
			Directory.CreateDirectory(privatePath);
			Directory.CreateDirectory(publicPath);

			Dictionary<string, object> parameters = new Dictionary<string, object>
				{
					{ "modulename", module.Name },
					{ "isprimary", false },
				};

			File.WriteAllText(Path.Combine(modulePath, module.Name + ".Build.cs")
				, Template.TransformToText<ModuleBuild_cs>(parameters));
			File.WriteAllText(Path.Combine(privatePath, module.Name + "PrivatePCH.h")
				, Template.TransformToText<PrivatePCH_h>(parameters));
			File.WriteAllText(Path.Combine(privatePath, module.Name + ".cpp")
				, Template.TransformToText<Module_cpp>(parameters));
			File.WriteAllText(Path.Combine(publicPath, module.Name + ".h")
				, Template.TransformToText<Module_h>(parameters));
		}
	}
}
