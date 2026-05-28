namespace ProObjLogLite;

public static class Logging
{
    private static readonly Random SamplingRandom = Random.Shared;
    public static readonly HashSet<string> KnownLevels =
    [
        "TRACE", "DEBUG", "INFO", "WARN", "ERROR", "FATAL"
    ];

    private static readonly Dictionary<string, int> LevelValue = new(StringComparer.OrdinalIgnoreCase)
    {
        ["TRACE"] = 0,
        ["DEBUG"] = 1,
        ["INFO"] = 2,
        ["WARN"] = 3,
        ["ERROR"] = 4,
        ["FATAL"] = 5
    };

    public static bool ShouldLog(Flags flags)
    {
        return LevelValue[flags.Level] >= LevelValue[flags.MinLevel];
    }

    public static bool PassesSampling(Flags flags)
    {
        if (flags.SampleRate >= 1)
            return true;

        return SamplingRandom.NextDouble() <= flags.SampleRate;
    }

    public static LogEntry CreateEntry(Flags flags)
    {
        var now = flags.Utc ? DateTime.UtcNow : DateTime.Now;

        return new LogEntry
        {
            Timestamp = now,
            Level = flags.Level,
            LogType = flags.LogType,
            Source = flags.Source,
            Message = flags.Message,
            Host = Environment.MachineName,
            ProcessId = Environment.ProcessId,
            EventId = flags.EventId,
            CorrelationId = flags.CorrelationId,
            Exception = flags.Exception,
            Count = flags.Count,
            Tags = ExtractTags(flags),
            Context = new Dictionary<string, string>(flags.Context)
        };
    }

    public static void PrintLog(LogEntry entry, Flags flags)
    {
        var formatter = ResolveFormatter(flags);
        Console.WriteLine(formatter.Format(entry, flags));
    }

    public static string SaveLog(LogEntry entry, Flags flags)
    {
        var filePath = ResolveOutputPath(flags, entry.Timestamp);

        entry = AttachChainHashes(entry, filePath, flags.ChainHash, persist: true);

        var formatter = ResolveFormatter(flags);
        var output = formatter.Format(entry, flags);
        if (flags.MaxSizeBytes is not null)
            RotateIfNeeded(filePath, flags.MaxSizeBytes.Value, flags.MaxFiles);

        AppendWithRetry(filePath, output + Environment.NewLine);
        return filePath;
    }

    public static LogEntry FinalizeForDisplay(LogEntry entry, Flags flags)
    {
        entry = Redact(entry, flags.RedactKeys);

        if (!flags.ChainHash)
            return entry;

        var targetPath = ResolveOutputPath(flags, entry.Timestamp);
        return AttachChainHashes(entry, targetPath, true, persist: false);
    }

    private static ILogFormatter ResolveFormatter(Flags flags)
    {
        return flags.JsonOutput ? new JsonLogFormatter() : new PrettyLogFormatter();
    }

    private static string ResolveOutputPath(Flags flags, DateTime now)
    {
        if (!string.IsNullOrWhiteSpace(flags.FilePath))
        {
            var explicitPath = Path.GetFullPath(flags.FilePath);
            var parent = Path.GetDirectoryName(explicitPath);
            if (!string.IsNullOrWhiteSpace(parent) && !Directory.Exists(parent))
                Directory.CreateDirectory(parent);
            return explicitPath;
        }

        if (!Directory.Exists(flags.Directory))
            Directory.CreateDirectory(flags.Directory);

        var fileName = now.ToString("yyyy_MM_dd") + ".log";
        return Path.Combine(flags.Directory, fileName);
    }

    private static void AppendWithRetry(string filePath, string content)
    {
        const int attempts = 5;

        for (var i = 1; i <= attempts; i++)
        {
            try
            {
                using var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                using var writer = new StreamWriter(stream);
                writer.Write(content);
                return;
            }
            catch (IOException) when (i < attempts)
            {
                Thread.Sleep(i * 40);
            }
        }

        throw new IOException($"Failed to write to log file '{filePath}' after multiple retries.");
    }

    private static void RotateIfNeeded(string filePath, long maxSizeBytes, int maxFiles)
    {
        if (!File.Exists(filePath))
            return;

        var info = new FileInfo(filePath);
        if (info.Length < maxSizeBytes)
            return;

        for (var i = maxFiles - 1; i >= 1; i--)
        {
            var src = $"{filePath}.{i}";
            var dst = $"{filePath}.{i + 1}";
            if (File.Exists(dst))
                File.Delete(dst);
            if (File.Exists(src))
                File.Move(src, dst);
        }

        var firstRotation = filePath + ".1";
        if (File.Exists(firstRotation))
            File.Delete(firstRotation);

        File.Move(filePath, firstRotation);
    }

    private static LogEntry AttachChainHashes(LogEntry entry, string filePath, bool enabled, bool persist)
    {
        if (!enabled)
            return entry;

        var previous = ReadPreviousHash(filePath);
        var canonical = BuildCanonical(entry, previous);
        var current = ComputeSha256(canonical);

        if (persist)
            WriteLatestHash(filePath, current);

        return new LogEntry
        {
            Timestamp = entry.Timestamp,
            Level = entry.Level,
            LogType = entry.LogType,
            Source = entry.Source,
            Message = entry.Message,
            Host = entry.Host,
            ProcessId = entry.ProcessId,
            EventId = entry.EventId,
            CorrelationId = entry.CorrelationId,
            Exception = entry.Exception,
            Count = entry.Count,
            Tags = [.. entry.Tags],
            Context = new Dictionary<string, string>(entry.Context),
            PreviousHash = previous,
            EntryHash = current
        };
    }

    private static string BuildCanonical(LogEntry entry, string previousHash)
    {
        var context = string.Join(";", entry.Context.OrderBy(k => k.Key).Select(kv => $"{kv.Key}={kv.Value}"));
        return string.Join("|",
            previousHash,
            entry.Timestamp.ToString("O"),
            entry.Level,
            entry.Source,
            entry.EventId ?? string.Empty,
            entry.CorrelationId ?? string.Empty,
            entry.Message,
            entry.Exception ?? string.Empty,
            entry.Count.ToString(),
            context);
    }

    private static string ComputeSha256(string input)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string ReadPreviousHash(string filePath)
    {
        var chainPath = filePath + ".chain";
        if (!File.Exists(chainPath))
            return "genesis";

        var previous = File.ReadAllText(chainPath).Trim();
        return previous.Length == 0 ? "genesis" : previous;
    }

    private static void WriteLatestHash(string filePath, string hash)
    {
        var chainPath = filePath + ".chain";
        File.WriteAllText(chainPath, hash + Environment.NewLine);
    }

    private static LogEntry Redact(LogEntry entry, string[] keys)
    {
        if (keys.Length == 0)
            return entry;

        var redactedContext = new Dictionary<string, string>(entry.Context, StringComparer.OrdinalIgnoreCase);
        foreach (var key in keys)
        {
            if (redactedContext.ContainsKey(key))
                redactedContext[key] = "[REDACTED]";
        }

        var redactedMessage = RedactText(entry.Message, keys);
        var redactedException = entry.Exception is null ? null : RedactText(entry.Exception, keys);

        return new LogEntry
        {
            Timestamp = entry.Timestamp,
            Level = entry.Level,
            LogType = entry.LogType,
            Source = entry.Source,
            Message = redactedMessage,
            Host = entry.Host,
            ProcessId = entry.ProcessId,
            EventId = entry.EventId,
            CorrelationId = entry.CorrelationId,
            Exception = redactedException,
            Count = entry.Count,
            Tags = [.. entry.Tags],
            Context = redactedContext,
            PreviousHash = entry.PreviousHash,
            EntryHash = entry.EntryHash
        };
    }

    private static List<string> ExtractTags(Flags flags)
    {
        if (!flags.Context.TryGetValue("smart.tags", out var tagsRaw))
            return [];

        return tagsRaw
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string RedactText(string value, string[] keys)
    {
        var output = value;
        foreach (var key in keys)
        {
            output = output.Replace($"{key}=", $"{key}=[REDACTED]", StringComparison.OrdinalIgnoreCase);
            output = output.Replace($"{key}:", $"{key}:[REDACTED]", StringComparison.OrdinalIgnoreCase);
        }

        return output;
    }
}
