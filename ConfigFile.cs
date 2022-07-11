using System;
using System.Collections.Generic;
using System.Text;
using SystemEx;

namespace UE4Assistant;

public class ConfigFile
{
	public class Section
	{
		public string name;
		public List<(string key, string value)> lines = new();

		public string this[string key] {
			get => lines.Find(s => s.key == name).value;
			set {
				var vi = lines.FindIndex(s => s.key == key);
				if (vi < 0)
					lines.Add((key, value));
				else
					lines[vi] = (key, value);
			}
		}
	}

	public List<Section> sections = new();

	public Section this[string name] {
		get => sections.Find(s => s.name == name);
	}

	public ConfigFile(string filename)
	{
		Section currentSection = null;

		foreach (var line in File.ReadLines(filename))
		{
			if (line.IsNullOrWhiteSpace())
				continue;

			if (line[0] == '[')
			{
				currentSection = new Section { name = line[1..line.IndexOf(']')] };
				sections.Add(currentSection);
			}
			else
			{
				var tokens = line.SplitFirst('=');
				currentSection.lines.Add((key: tokens[0], value: tokens[1]));
			}
		}
	}

	protected IEnumerable<string> EmitLines()
	{
		foreach (var section in sections)
		{
			yield return $"[{section.name}]";
			foreach (var line in section.lines)
			{
				yield return $"{line.key}={line.value}";
			}

			yield return string.Empty;
		}
	}

	public void Save(string filename)
	{
		File.WriteAllLines(filename, EmitLines());
	}

	public Section AddSection(string name)
	{
		var section = new Section { name = name };

		sections.Add(section);

		return section;
	}
}
