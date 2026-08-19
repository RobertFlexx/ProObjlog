using System.Text;
using System.Text.Json;

namespace ProObjLogLite;

public interface ILogFormatter
{
    string Format(LogEntry entry, Flags flags);
}

public sealed class JsonLogFormatter : ILogFormatter
{
    public string Format(LogEntry entry, Flags flags)
    {
        var payload = new LogPayload(
            entry.Timestamp.ToString(flags.TimestampFormat),
            entry.Level,
            entry.LogType,
            entry.Source,
            entry.EventId,
            entry.CorrelationId,
            entry.Message,
            entry.Exception,
            entry.Count,
            entry.PreviousHash,
            entry.EntryHash,
            entry.Host,
            entry.ProcessId,
            entry.Tags,
            entry.Context);

        return JsonSerializer.Serialize(payload, JsonContext.Default.LogPayload);
    }
}

public sealed class PrettyLogFormatter : ILogFormatter
{
    public string Format(LogEntry entry, Flags flags)
    {
        var sb = new StringBuilder();
        var symbol = entry.Level switch
        {
            "TRACE" => "~",
            "DEBUG" => "*",
            "INFO" => "+",
            "WARN" => "!",
            "ERROR" => "x",
            "FATAL" => "X",
            _ => ">"
        };

        sb.Append(Color.Blue("["));
        sb.Append(Color.Magenta(entry.Timestamp.ToString(flags.TimestampFormat)));
        sb.Append(Color.Blue("] "));
        sb.Append(ColorForLevel(entry.Level, symbol + " " + entry.Level));
        sb.Append(Color.Blue("  type="));
        sb.Append(Color.Yellow(entry.LogType));
        sb.Append(Color.Blue("  src="));
        sb.Append(Color.Green(entry.Source));

        if (!string.IsNullOrWhiteSpace(entry.EventId))
        {
            sb.Append(Color.Blue("  event="));
            sb.Append(Color.Yellow(entry.EventId));
        }

        if (!string.IsNullOrWhiteSpace(entry.CorrelationId))
        {
            sb.Append(Color.Blue("  cid="));
            sb.Append(Color.Magenta(entry.CorrelationId));
        }

        sb.Append(Color.Blue("\nmessage: "));
        sb.Append(Color.Magenta(entry.Message));

        if (entry.Count > 1)
        {
            sb.Append(Color.Blue("\ncount: "));
            sb.Append(Color.Yellow(entry.Count.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(entry.Exception))
        {
            sb.Append(Color.Blue("\nexception: "));
            sb.Append(Color.Red(entry.Exception));
        }

        if (entry.Context.Count > 0)
        {
            sb.Append(Color.Blue("\ncontext: "));
            sb.Append(string.Join(", ", entry.Context.Select(kv => $"{kv.Key}={kv.Value}")));
        }

        if (entry.Tags.Count > 0)
        {
            sb.Append(Color.Blue("\ntags: "));
            sb.Append(string.Join(", ", entry.Tags));
        }

        if (!string.IsNullOrWhiteSpace(entry.EntryHash))
        {
            sb.Append(Color.Blue("\nchain: "));
            sb.Append(Color.Green($"prev={Short(entry.PreviousHash)} current={Short(entry.EntryHash)}"));
        }

        return sb.ToString();
    }

    private static string Short(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "none";

        return value.Length <= 12 ? value : value[..12];
    }

    private static string ColorForLevel(string level, string value)
    {
        return level switch
        {
            "TRACE" => Color.Blue(value),
            "DEBUG" => Color.Magenta(value),
            "INFO" => Color.Green(value),
            "WARN" => Color.Yellow(value),
            "ERROR" => Color.Red(value),
            "FATAL" => Color.Red(value),
            _ => Color.Blue(value)
        };
    }
}
