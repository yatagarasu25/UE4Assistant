using ANSIConsole;
using SystemEx;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UE4Assistant;

public class UnrealLogPrinter
{
	public struct ColorPattern
	{
		public string pattern;
		public Tuple<ConsoleColor, ConsoleColor>[] colors;
	}

	public static List<ColorPattern> patterns = new ();
	public static Regex pattern;

	static UnrealLogPrinter()
	{
		patterns.Add(new ColorPattern { pattern = @"(Running):\s*(.*)"
				, colors = new[] {
					Tuple.Create(Console.ForegroundColor, Console.BackgroundColor)
					, Tuple.Create(ConsoleColor.DarkGray, Console.BackgroundColor)
				}
		});
		patterns.Add(new ColorPattern {	pattern = @"(Warning):\s*(.*)"
				, colors = new[] {
					Tuple.Create(ConsoleColor.Yellow, Console.BackgroundColor)
					, Tuple.Create(ConsoleColor.DarkGray, Console.BackgroundColor)
				}
			}
		);
		patterns.Add(new ColorPattern {	pattern = @"(Error):\s*(.*)"
				, colors = new[] {
					Tuple.Create(ConsoleColor.Red, Console.BackgroundColor)
					, Tuple.Create(ConsoleColor.DarkGray, Console.BackgroundColor)
				}
			}
		);

		var patternStr = $"^.*?{patterns.Select((p, i) => $"(?<g{i}>{p.pattern})").Join('|')}.*?$";
		pattern = new Regex(patternStr, RegexOptions.Compiled);
	}

	public static void WriteLine(string str)
	{
		var m = pattern.Match(str);
		if (m.Success)
		{
			var patternGroup = m.Groups.Cast<Group>().Where(k => !k.Value.IsNullOrEmpty() && k.Name.StartsWith("g")).First();
			var patternIndex = int.Parse(patternGroup.Name[1..]);
			var pattern = patterns[patternIndex];

			var groups = m.Groups.Cast<Group>().Where(g => !g.Value.IsNullOrEmpty() && g != patternGroup).ToArray();
			for (int i = groups.Length - 1; i > 0; i--)
			{
				var group = groups[i];
				if (group == patternGroup)
					continue;

				var colors = pattern.colors[i - 1];
				var groupStr = group.Value.Color(colors.Item1).Background(colors.Item2).ToString();
				str = str.Remove(group.Index, group.Length).Insert(group.Index, groupStr);
			}

			Console.WriteLine(str);
		}
		else
			Console.WriteLine(str);
	}
}
