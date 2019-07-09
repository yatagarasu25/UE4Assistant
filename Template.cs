using System.Collections.Generic;



namespace UE4Assistant
{
	public static class Template
	{
		public static string TransformToText<TemplateType>(Dictionary<string, object> parameters) where TemplateType : new()
		{
			var TemplateInstance = new TemplateType();
			typeof(TemplateType).GetProperty("Session").SetValue(TemplateInstance, parameters);
			typeof(TemplateType).GetMethod("Initialize").Invoke(TemplateInstance, null);

			return (string)typeof(TemplateType).GetMethod("TransformText").Invoke(TemplateInstance, null);
		}
	}
}
