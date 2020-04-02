using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;



namespace UE4Assistant
{
	public static class Utilities
	{
		public static DateTime GetLinkerTime(this Assembly assembly, TimeZoneInfo target = null)
		{
			var filePath = assembly.Location;
			const int c_PeHeaderOffset = 60;
			const int c_LinkerTimestampOffset = 8;

			var buffer = new byte[2048];

			using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				stream.Read(buffer, 0, 2048);

			var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
			var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

			var tz = target ?? TimeZoneInfo.Local;
			var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

			return localTime;
		}

		public static string EscapeCommandLineArgs(params string[] args)
		{
			return string.Join(" ", args.Select((a) =>
			{
				var s = Regex.Replace(a, @"(\\*)" + "\"", @"$1$1\" + "\"");
				if (s.Contains(" "))
				{
					s = "\"" + Regex.Replace(s, @"(\\+)$", @"$1$1") + "\"";
				}
				return s;
			}).ToArray());
		}

		public static void DeleteFile(string path)
		{
			Console.WriteLine("rm " + path);
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}

		public static void DeleteDirectory(string path)
		{
			Console.WriteLine("rm " + path);
			if (Directory.Exists(path))
			{
				Directory.Delete(path, true);
			}
		}

		public static void ExecuteCommandLine(string command)
		{
			Console.WriteLine("> " + command);

			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.UseShellExecute = false;
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				processStartInfo.FileName = "cmd.exe";
				processStartInfo.Arguments = "/C \"" + command + "\"";
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				processStartInfo.FileName = "sh";
				processStartInfo.Arguments = command;
			}

			using (Process process = Process.Start(processStartInfo))
			{
				//process.StartInfo.RedirectStandardOutput = true;
				//process.StartInfo.RedirectStandardError = true;
				//process.StartInfo.RedirectStandardInput = true;
				//process.BeginOutputReadLine();
				//process.BeginErrorReadLine();

				//Console.Write(process.StandardOutput.ReadToEnd());
				//Console.Write(process.StandardError.ReadToEnd());

				process.WaitForExit();
			}
		}

		public static void ExecuteOpenFile(string filename)
		{
			Console.WriteLine("> " + filename);

			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.UseShellExecute = true;
			processStartInfo.WorkingDirectory = Path.GetDirectoryName(filename);
			processStartInfo.FileName = filename;
			processStartInfo.Verb = "OPEN";
			using (Process process = Process.Start(processStartInfo))
			{
				//process.StartInfo.RedirectStandardOutput = true;
				//process.StartInfo.RedirectStandardError = true;
				//process.StartInfo.RedirectStandardInput = true;
				//process.BeginOutputReadLine();
				//process.BeginErrorReadLine();

				//Console.Write(process.StandardOutput.ReadToEnd());
				//Console.Write(process.StandardError.ReadToEnd());
			}
		}

		public static byte[] CalculateMD5(string filename)
		{
			using (var md5 = MD5.Create())
			{
				using (var stream = File.OpenRead(filename))
				{
					return md5.ComputeHash(stream);
				}
			}
		}

		/*
		public static List<string> ListSubmodulePaths(string gitpath)
		{
			List<string> submodules = new List<string>();

			try
			{
				ListSubmodulePaths(new Repository(gitpath), submodules);
			}
			catch { }

			return submodules;
		}

		public static void ListSubmodulePaths(Repository repository, List<string> result)
		{
			foreach (var submodule in repository.Submodules)
			{
				try { ListSubmodulePaths(new Repository(Path.Combine(repository.Info.WorkingDirectory, submodule.Path)), result); } catch { }
			}

			result.Add(repository.Info.WorkingDirectory);
		}
		*/

		public static string ClampPath(string fullpath, string prefixpath)
		{
			int prefixLength = prefixpath.Length + 1;

			if (fullpath.Length > prefixLength)
				return fullpath.Substring(prefixLength) + "/";

			return string.Empty;
		}

		public static string GetModuleNameFromPackageName(this string packageName)
		{
			int i0 = packageName.IndexOf('/');
			int i1 = packageName.IndexOf('/', i0 + 1);

			return packageName.Substring(i0 + 1, i1 - i0 - 1);
		}

		public static string GetObjectNameFromPackageName(this string packageName)
		{
			int i0 = packageName.LastIndexOf('/');

			return packageName.Substring(i0 + 1);
		}

		class PreventSleepGuard : IDisposable
		{
			public PreventSleepGuard()
			{
				SetThreadExecutionState(ExecutionState.EsContinuous | ExecutionState.EsSystemRequired);
			}

			public void Dispose()
			{
				SetThreadExecutionState(ExecutionState.EsContinuous);
			}
		}

		public static IDisposable PreventSleep()
		{
			return new PreventSleepGuard();
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern ExecutionState SetThreadExecutionState(ExecutionState esFlags);

		[FlagsAttribute]
		private enum ExecutionState : uint
		{
			EsAwaymodeRequired = 0x00000040,
			EsContinuous = 0x80000000,
			EsDisplayRequired = 0x00000002,
			EsSystemRequired = 0x00000001
		}

		class SetCurrentDirectoryGuard : IDisposable
		{
			string olddir;

			public SetCurrentDirectoryGuard(string newdir)
			{
				olddir = Directory.GetCurrentDirectory();
				Directory.SetCurrentDirectory(newdir);
			}

			public void Dispose()
			{
				Directory.SetCurrentDirectory(olddir);
			}
		}

		public static IDisposable SetCurrentDirectory(string newdir)
		{
			return new SetCurrentDirectoryGuard(newdir);
		}

		public static IEnumerable<string> ParsePCHFilesErrors(this string log)
		{
			foreach (string line in log.Split('\n'))
			{
				int mi = line.IndexOf("fatal error C1853");
				if (mi < 0)
					continue;

				int si = line.IndexOf('\'', mi);
				if (si < 0)
					continue;

				int ei = line.IndexOf('\'', si + 1);
				if (ei <= si)
					continue;

				yield return line.Substring(si + 1, ei - si - 1);
			}
		}
	}
}
