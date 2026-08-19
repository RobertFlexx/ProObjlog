using System.Text.Json.Serialization;

namespace ProObjLogLite;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(Flags))]
[JsonSerializable(typeof(Dictionary<string, DateTime>))]
[JsonSerializable(typeof(LogPayload))]
internal sealed partial class JsonContext : JsonSerializerContext
{
}

public sealed record LogPayload(
    string timestamp,
    string level,
    string logType,
    string source,
    string? eventId,
    string? correlationId,
    string message,
    string? exception,
    int count,
    string? previousHash,
    string? entryHash,
    string host,
    int processId,
    List<string> tags,
    Dictionary<string, string> context);