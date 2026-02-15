using System.Text.Json;
using System.Text.Json.Serialization;

public static class JsonUtil
{
	public static readonly JsonSerializerOptions Options = new()
	{
		WriteIndented = true,
		AllowTrailingCommas = true,
		ReadCommentHandling = JsonCommentHandling.Skip,
		PropertyNameCaseInsensitive = true,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
	};
}
