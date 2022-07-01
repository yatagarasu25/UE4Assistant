namespace UE4Assistant;

public class UE4AssistantException : ApplicationException
{
	public UE4AssistantException(int errorCode = -1)
		=> HResult = errorCode;

	public UE4AssistantException(string message, int errorCode = -1)
		: base(message)
		=> HResult = errorCode;
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
	private string command;

	public override string Message => $"Failed to run command \"{command}\" with error code {HResult}.";

	public ExecuteCommandLineException(string command, int errorCode)
		: base(errorCode)
	{
		this.command = command;
	}
}

public class UERootNotFound : UE4AssistantException
{
	private string path;

	public override string Message => $"Engine root for {path} does not exist.";

	public UERootNotFound(string path)
	{
		this.path = path;
	}
}

public class UEIdNotFound : UE4AssistantException
{
	private string id;

	public override string Message => $"Engine root for {id} not found in registry.";

	public UEIdNotFound(string id)
	{
		this.id = id;
	}
}
