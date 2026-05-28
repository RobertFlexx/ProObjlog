namespace ProObjLogLite;

public static class LogEngine
{
    public static bool ShouldSkip(Flags flags)
    {
        if (string.IsNullOrWhiteSpace(flags.When))
            return false;

        return !Evaluate(flags.When, flags.Context);
    }

    public static void Apply(Flags flags)
    {
        if (!string.IsNullOrWhiteSpace(flags.Decision))
        {
            flags.LogType = "decision";
            flags.Context["decision"] = flags.Decision;
            if (!string.IsNullOrWhiteSpace(flags.Outcome))
                flags.Context["outcome"] = flags.Outcome;
        }

        foreach (var metricPair in flags.Metrics)
        {
            var idx = metricPair.IndexOf('=');
            if (idx <= 0 || idx == metricPair.Length - 1)
                continue;

            var key = metricPair[..idx].Trim();
            var value = metricPair[(idx + 1)..].Trim();
            flags.Context[$"metric.{key}"] = value;
            flags.LogType = "metric";
        }

        if (!string.IsNullOrWhiteSpace(flags.Assert))
        {
            flags.LogType = "logic";
            var ok = Evaluate(flags.Assert, flags.Context);
            flags.Context["assert"] = flags.Assert;
            flags.Context["assert_result"] = ok ? "pass" : "fail";
            if (!ok)
                flags.Level = "FATAL";
        }

        if (flags.Smart)
            ApplySmart(flags);
    }

    private static void ApplySmart(Flags flags)
    {
        var m = flags.Message.ToLowerInvariant();
        var tags = new List<string>();

        if (m.Contains("timeout") || m.Contains("failed") || m.Contains("exception"))
        {
            flags.Level = "ERROR";
            tags.Add("failure");
        }
        else if (m.Contains("warn") || m.Contains("slow"))
        {
            flags.Level = "WARN";
            tags.Add("degraded");
        }
        else if (m.Contains("success") || m.Contains("completed"))
        {
            flags.Level = "INFO";
            tags.Add("success");
        }

        if (m.Contains("auth") || m.Contains("login"))
            tags.Add("security");
        if (m.Contains("payment") || m.Contains("billing"))
            tags.Add("finance");

        if (tags.Count > 0)
            flags.Context["smart.tags"] = string.Join(",", tags.Distinct());
    }

    private static bool Evaluate(string expression, Dictionary<string, string> context)
    {
        var parts = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
            throw new ArgumentException("Expression must be: key op value");

        var key = parts[0];
        var op = parts[1];
        var expected = parts[2];

        context.TryGetValue(key, out var actual);
        actual ??= string.Empty;

        return op switch
        {
            "==" => string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
            "!=" => !string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
            "contains" => actual.Contains(expected, StringComparison.OrdinalIgnoreCase),
            _ => throw new ArgumentException("Expression operator must be ==, !=, or contains")
        };
    }
}
