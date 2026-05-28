using System.Text.Json;

namespace ProObjLogLite;

public class Flags
{
    public string Level { get; set; } = "INFO";
    public string Message { get; set; } = "No message provided";
    public string Directory { get; set; } = "logs";
    public bool NoPrint { get; set; }
    public bool HelpRequested { get; set; }
    public bool VersionRequested { get; set; }
    public bool ListLevelsRequested { get; set; }
    public bool InitConfigRequested { get; set; }
    public bool JsonOutput { get; set; }
    public string Source { get; set; } = "default";
    public string? FilePath { get; set; }
    public string MinLevel { get; set; } = "TRACE";
    public bool Utc { get; set; }
    public string TimestampFormat { get; set; } = "yyyy-MM-ddTHH:mm:ss.fffK";
    public bool NoColor { get; set; }
    public bool StdoutOnly { get; set; }
    public bool DryRun { get; set; }
    public long? MaxSizeBytes { get; set; }
    public int MaxFiles { get; set; } = 7;
    public string? EventId { get; set; }
    public string? CorrelationId { get; set; }
    public string? Exception { get; set; }
    public Dictionary<string, string> Context { get; set; } = [];
    public int Count { get; set; } = 1;
    public bool ChainHash { get; set; }
    public string? TimerName { get; set; }
    public bool TimerStart { get; set; }
    public bool TimerStop { get; set; }
    public string? Profile { get; set; }
    public double SampleRate { get; set; } = 1.0;
    public string[] RedactKeys { get; set; } = [];
    public string LogType { get; set; } = "event";
    public bool Smart { get; set; }
    public string? Assert { get; set; }
    public string? When { get; set; }
    public string[] Metrics { get; set; } = [];
    public string? Decision { get; set; }
    public string? Outcome { get; set; }
    public bool AsyncWrite { get; set; } = true;
    public bool FireAndForget { get; set; }

    public static Flags Parse(string[] args)
    {
        var flags = LoadConfig();

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (!arg.StartsWith('-'))
                continue;

            switch (arg)
            {
                case "--help":
                case "-h":
                    flags.HelpRequested = true;
                    return flags;
                case "--version":
                case "-v":
                    flags.VersionRequested = true;
                    return flags;
                case "--list-levels":
                    flags.ListLevelsRequested = true;
                    return flags;
                case "--init-config":
                    flags.InitConfigRequested = true;
                    return flags;
                case "--profile":
                    flags.Profile = ReadFlagValue(args, ref i, "profile");
                    ApplyProfile(flags);
                    break;
                case "--type":
                    flags.LogType = ReadFlagValue(args, ref i, "log type").ToLowerInvariant();
                    break;
                case "--smart":
                    flags.Smart = true;
                    break;
                case "--assert":
                    flags.Assert = ReadFlagValue(args, ref i, "assert expression");
                    break;
                case "--when":
                    flags.When = ReadFlagValue(args, ref i, "when expression");
                    break;
                case "--metric":
                    flags.Metrics = flags.Metrics.Append(ReadFlagValue(args, ref i, "metric pair")).ToArray();
                    break;
                case "--decision":
                    flags.Decision = ReadFlagValue(args, ref i, "decision name");
                    break;
                case "--outcome":
                    flags.Outcome = ReadFlagValue(args, ref i, "decision outcome");
                    break;
                case "--sync":
                    flags.AsyncWrite = false;
                    break;
                case "--fire-and-forget":
                    flags.FireAndForget = true;
                    break;
                case "-l":
                case "--level":
                    flags.Level = ReadFlagValue(args, ref i, "level").ToUpperInvariant();
                    break;
                case "-m":
                case "--message":
                    flags.Message = ReadFlagValue(args, ref i, "message");
                    break;
                case "-d":
                case "--dir":
                    flags.Directory = ReadFlagValue(args, ref i, "directory");
                    break;
                case "-n":
                case "--noprint":
                    flags.NoPrint = true;
                    break;
                case "--json":
                    flags.JsonOutput = true;
                    break;
                case "-s":
                case "--source":
                case "--tag":
                    flags.Source = ReadFlagValue(args, ref i, "source");
                    break;
                case "-f":
                case "--file":
                    flags.FilePath = ReadFlagValue(args, ref i, "file path");
                    break;
                case "--min-level":
                    flags.MinLevel = ReadFlagValue(args, ref i, "minimum level").ToUpperInvariant();
                    break;
                case "--utc":
                    flags.Utc = true;
                    break;
                case "--timestamp-format":
                    flags.TimestampFormat = ReadFlagValue(args, ref i, "timestamp format");
                    break;
                case "--no-color":
                    flags.NoColor = true;
                    break;
                case "--max-size":
                    flags.MaxSizeBytes = ParseSize(ReadFlagValue(args, ref i, "max size"));
                    break;
                case "--max-files":
                    flags.MaxFiles = int.Parse(ReadFlagValue(args, ref i, "max files"));
                    break;
                case "--stdout-only":
                    flags.StdoutOnly = true;
                    break;
                case "--dry-run":
                    flags.DryRun = true;
                    break;
                case "--event-id":
                    flags.EventId = ReadFlagValue(args, ref i, "event id");
                    break;
                case "--correlation-id":
                case "--cid":
                    flags.CorrelationId = ReadFlagValue(args, ref i, "correlation id");
                    break;
                case "--exception":
                    flags.Exception = ReadFlagValue(args, ref i, "exception details");
                    break;
                case "--context":
                    AddContext(flags.Context, ReadFlagValue(args, ref i, "context pair"));
                    break;
                case "--count":
                    flags.Count = int.Parse(ReadFlagValue(args, ref i, "count"));
                    break;
                case "--chain-hash":
                    flags.ChainHash = true;
                    break;
                case "--timer":
                    flags.TimerName = ReadFlagValue(args, ref i, "timer name");
                    break;
                case "--timer-start":
                    flags.TimerStart = true;
                    break;
                case "--timer-stop":
                    flags.TimerStop = true;
                    break;
                case "--sample-rate":
                    flags.SampleRate = double.Parse(ReadFlagValue(args, ref i, "sample rate"));
                    break;
                case "--redact-keys":
                    flags.RedactKeys = ReadFlagValue(args, ref i, "redact keys")
                        .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    break;
                default:
                    throw new Exception($"Unknown flag {arg}.");
            }
        }

        if (flags.Message == "-")
            flags.Message = Console.In.ReadToEnd();

        if (flags.MaxFiles < 1)
            throw new ArgumentException("--max-files must be at least 1.");

        if (flags.Count < 1)
            throw new ArgumentException("--count must be at least 1.");

        if ((flags.TimerStart || flags.TimerStop) && string.IsNullOrWhiteSpace(flags.TimerName))
            throw new ArgumentException("--timer is required when using --timer-start or --timer-stop.");

        if (flags.TimerStart && flags.TimerStop)
            throw new ArgumentException("Use only one of --timer-start or --timer-stop.");

        if (!Logging.KnownLevels.Contains(flags.Level))
            throw new ArgumentException($"Invalid level '{flags.Level}'. Use TRACE, DEBUG, INFO, WARN, ERROR, or FATAL.");

        if (!Logging.KnownLevels.Contains(flags.MinLevel))
            throw new ArgumentException($"Invalid --min-level '{flags.MinLevel}'.");

        if (flags.SampleRate is < 0 or > 1)
            throw new ArgumentException("--sample-rate must be between 0 and 1.");

        if (!flags.AsyncWrite && flags.FireAndForget)
            throw new ArgumentException("--fire-and-forget requires async mode (remove --sync).");

        if (flags.LogType is not ("event" or "audit" or "metric" or "decision" or "logic"))
            throw new ArgumentException("--type must be one of: event, audit, metric, decision, logic.");

        return flags;
    }

    private static void ApplyProfile(Flags flags)
    {
        var profile = flags.Profile?.Trim().ToLowerInvariant();
        switch (profile)
        {
            case "ci":
                flags.JsonOutput = true;
                flags.Utc = true;
                flags.NoColor = true;
                flags.Source = "ci";
                flags.MinLevel = "INFO";
                break;
            case "prod":
                flags.JsonOutput = true;
                flags.Utc = true;
                flags.NoColor = true;
                flags.ChainHash = true;
                flags.MinLevel = "WARN";
                break;
            case "dev":
                flags.JsonOutput = false;
                flags.Utc = false;
                flags.NoColor = false;
                flags.MinLevel = "TRACE";
                break;
            case null:
            case "":
                break;
            default:
                throw new ArgumentException("Unknown profile. Use dev, ci, or prod.");
        }
    }

    private static Flags LoadConfig()
    {
        var path = Path.Combine(Environment.CurrentDirectory, "proobjloglite.json");
        if (!File.Exists(path))
            return new Flags();

        var json = File.ReadAllText(path);
        var loaded = JsonSerializer.Deserialize<Flags>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return loaded ?? new Flags();
    }

    private static string ReadFlagValue(string[] args, ref int index, string valueName)
    {
        if (index + 1 >= args.Length)
            throw new ArgumentException($"No {valueName} provided after flag");

        var value = args[index + 1];
        if (IsKnownFlag(value))
            throw new ArgumentException($"No {valueName} provided after flag");

        index++;
        return value;
    }

    private static long ParseSize(string input)
    {
        var normalized = input.Trim().ToUpperInvariant();
        if (normalized.EndsWith("KB"))
            return ParseLeadingNumber(normalized, "KB") * 1024L;
        if (normalized.EndsWith("MB"))
            return ParseLeadingNumber(normalized, "MB") * 1024L * 1024L;
        if (normalized.EndsWith("GB"))
            return ParseLeadingNumber(normalized, "GB") * 1024L * 1024L * 1024L;
        if (long.TryParse(normalized, out var plainBytes) && plainBytes > 0)
            return plainBytes;

        throw new ArgumentException("Invalid --max-size. Use forms like 1024, 256KB, 10MB, 1GB.");
    }

    private static long ParseLeadingNumber(string value, string suffix)
    {
        var number = value[..^suffix.Length].Trim();
        if (!long.TryParse(number, out var parsed) || parsed < 1)
            throw new ArgumentException("Invalid --max-size value.");

        return parsed;
    }

    private static bool IsKnownFlag(string value)
    {
        return value is "-h" or "--help" or "-l" or "--level" or "-m" or "--message" or "-d" or "--dir" or
            "-n" or "--noprint" or "--json" or "-s" or "--source" or "--tag" or "-f" or "--file" or
            "--min-level" or "--utc" or "--timestamp-format" or "--no-color" or "--max-size" or "--max-files" or
            "--stdout-only" or "--event-id" or "--correlation-id" or "--cid" or "--exception" or "--context" or
            "--version" or "-v" or "--list-levels" or "--init-config" or "--dry-run" or "--count" or
            "--chain-hash" or "--timer" or "--timer-start" or "--timer-stop" or "--profile" or
            "--sample-rate" or "--redact-keys" or "--type" or "--smart" or "--assert" or "--when" or
            "--metric" or "--decision" or "--outcome" or "--sync" or "--fire-and-forget";
    }

    private static void AddContext(Dictionary<string, string> context, string pair)
    {
        var separator = pair.IndexOf('=');
        if (separator <= 0 || separator == pair.Length - 1)
            throw new ArgumentException("Invalid --context value. Use key=value.");

        var key = pair[..separator].Trim();
        var value = pair[(separator + 1)..].Trim();
        if (key.Length == 0 || value.Length == 0)
            throw new ArgumentException("Invalid --context value. Use key=value.");

        context[key] = value;
    }

    public static void PrintHelp()
    {
        Console.WriteLine(Color.Blue("ProObjLogLite - Fast CLI logger"));
        Console.WriteLine(Color.Green("USAGE: ProObjLogLite [OPTIONS]"));
        Console.WriteLine();
        Console.WriteLine(Color.Yellow("Core"));
        PrintFlag("-d, --dir <path>", "Directory for date-based logs (default: logs)");
        PrintFlag("-l, --level <lvl>", "TRACE|DEBUG|INFO|WARN|ERROR|FATAL (default: INFO)");
        PrintFlag("-m, --message <msg>", "Message text. Use '-' to read stdin");
        PrintFlag("-n, --noprint", "Write file only, skip console output");
        PrintFlag("-h, --help", "Show this help message");
        PrintFlag("-v, --version", "Show current version");
        PrintFlag("--list-levels", "Show all supported levels");
        PrintFlag("--init-config", "Create proobjloglite.json template");
        Console.WriteLine();
        Console.WriteLine(Color.Yellow("Formatting"));
        PrintFlag("--json", "Use JSON log format");
        PrintFlag("-s, --source <name>", "Attach source/tag to each log");
        PrintFlag("--utc", "Use UTC timestamps");
        PrintFlag("--timestamp-format <fmt>", "Custom DateTime format string");
        PrintFlag("--no-color", "Disable ANSI colors");
        PrintFlag("--stdout-only", "Print only, do not write to file");
        PrintFlag("--dry-run", "Format and print without writing files");
        Console.WriteLine();
        Console.WriteLine(Color.Yellow("Storage"));
        PrintFlag("-f, --file <path>", "Explicit output file path");
        PrintFlag("--min-level <lvl>", "Skip writing logs below this level");
        PrintFlag("--max-size <bytes|KB|MB|GB>", "Rotate file when size is exceeded");
        PrintFlag("--max-files <n>", "Keep latest n rotated files (default: 7)");
        Console.WriteLine();
        Console.WriteLine(Color.Yellow("Advanced"));
        PrintFlag("--event-id <id>", "Attach event id for tracing");
        PrintFlag("--correlation-id <id>", "Attach correlation id (alias: --cid)");
        PrintFlag("--exception <text>", "Attach exception details");
        PrintFlag("--context key=value", "Add metadata context; repeatable");
        PrintFlag("--count <n>", "Record one log event repeated n times");
        PrintFlag("--chain-hash", "Append tamper-evident hash chain metadata");
        PrintFlag("--timer <name>", "Timer label for duration tracking");
        PrintFlag("--timer-start", "Start named timer");
        PrintFlag("--timer-stop", "Stop named timer and log elapsed time");
        PrintFlag("--profile <dev|ci|prod>", "Apply tuned defaults for environment");
        PrintFlag("--sample-rate <0..1>", "Probabilistic log sampling");
        PrintFlag("--redact-keys a,b,c", "Redact secrets from context/message/exception");
        PrintFlag("--type <event|audit|metric|decision|logic>", "Set log entry type");
        PrintFlag("--smart", "Enable smart level/tag inference");
        PrintFlag("--assert <expr>", "Logic assertion, upgrades failures to FATAL");
        PrintFlag("--when <expr>", "Log only when expression evaluates true");
        PrintFlag("--metric name=value", "Attach numeric metric; repeatable");
        PrintFlag("--decision <name>", "Decision log type helper");
        PrintFlag("--outcome <value>", "Decision outcome value");
        PrintFlag("--sync", "Disable async writer and use blocking file writes");
        PrintFlag("--fire-and-forget", "Queue async write and return immediately");
        Console.WriteLine();
        Console.WriteLine(Color.Green("Config file:"));
        Console.WriteLine("  proobjloglite.json in current directory is loaded before CLI flags.");
    }

    private static void PrintFlag(string flag, string description)
    {
        Console.Write(Color.Magenta($"  {flag}"));
        Console.WriteLine(Color.Blue($"  -> {description}"));
    }
}
