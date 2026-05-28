namespace ProObjLogLite;

public sealed class LogEntry
{
    public required DateTime Timestamp { get; init; }
    public required string Level { get; init; }
    public required string LogType { get; init; }
    public required string Source { get; init; }
    public required string Message { get; init; }
    public required string Host { get; init; }
    public required int ProcessId { get; init; }
    public string? EventId { get; init; }
    public string? CorrelationId { get; init; }
    public string? Exception { get; init; }
    public int Count { get; init; } = 1;
    public string? PreviousHash { get; init; }
    public string? EntryHash { get; init; }
    public List<string> Tags { get; init; } = [];
    public Dictionary<string, string> Context { get; init; } = [];
}
