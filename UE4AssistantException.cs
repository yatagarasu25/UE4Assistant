using System;
using System.Linq;
using SystemEx;

namespace UE4Assistant
{
	public class UE4AssistantException : ApplicationException
	{
		public int errorCode;

		public UE4AssistantException(int errorCode = -1)
		{
			this.errorCode = errorCode;
		}

		public UE4AssistantException(string message, int errorCode = -1)
			: base(message)
		{
			this.errorCode = errorCode;
		}
	}

	public class RequireUnrealItemException : UE4AssistantException
	{
		private string path;
		private UnrealItemType[] types;

		public override string Message => $"Can not find any suitable unreal item in ${path} ot it's parent folders. Require any of ${types.Select(t => t.ToString()).Join(", ")}.";

		public RequireUnrealItemException(string path, params UnrealItemType[] types)
		{
			this.path = path;
			this.types = types;
		}
	}

	public class ExecuteCommandLineException : UE4AssistantException
	{
		public ExecuteCommandLineException(int errorCode)
			: base(errorCode)
		{
		}
	}

}
