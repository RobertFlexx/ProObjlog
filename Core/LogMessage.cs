using System;
using System.Globalization;

namespace ProObjLog.Core;

public abstract class LogMessage
{
    public string Level { get; protected set; } = "INFO";
    public string Message { get; protected set; }
    public DateTimeOffset Timestamp { get; }
    public string Uuid { get; }

    protected LogMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            message = "N/A";

        Message = message;
        Timestamp = DateTimeOffset.Now;
        Uuid = $"{Timestamp.ToUnixTimeMilliseconds()}-{Random.Shared.Next(0, 1000)}";
    }

    public virtual string Format()
    {
        var formatted = Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
        return $"[{formatted}] {Level}: {Message}";
    }

    public abstract string Colorize(string text);

    public override string ToString() => Format();
}
