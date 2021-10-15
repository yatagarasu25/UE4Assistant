using Newtonsoft.Json;
using System.IO;

namespace UE4Assistant
{
	public class Configuration
	{
		public static T ReadConfiguration<T>(string path)
			where T : new()
		{
			return File.Exists(path)
				? JsonConvert.DeserializeObject<T>(File.ReadAllText(path))
				: new T();
		}

		public static void WriteConfiguration<T>(string path, T configuration)
		{
			File.WriteAllText(path, JsonConvert.SerializeObject(configuration, Formatting.Indented));
		}
	}
}
