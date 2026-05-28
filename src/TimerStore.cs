using System.Text.Json;

namespace ProObjLogLite;

public static class TimerStore
{
    private const string FileName = ".proobjloglite_timers.json";

    public static void Start(string name, DateTime startedAt)
    {
        var timers = Load();
        timers[name] = startedAt;
        Save(timers);
    }

    public static TimeSpan Stop(string name, DateTime endedAt)
    {
        var timers = Load();
        if (!timers.TryGetValue(name, out var startedAt))
            throw new InvalidOperationException($"Timer '{name}' has not been started.");

        timers.Remove(name);
        Save(timers);
        return endedAt - startedAt;
    }

    private static Dictionary<string, DateTime> Load()
    {
        var path = Path.Combine(Environment.CurrentDirectory, FileName);
        if (!File.Exists(path))
            return new Dictionary<string, DateTime>(StringComparer.Ordinal);

        var json = File.ReadAllText(path);
        var parsed = JsonSerializer.Deserialize<Dictionary<string, DateTime>>(json);
        return parsed ?? new Dictionary<string, DateTime>(StringComparer.Ordinal);
    }

    private static void Save(Dictionary<string, DateTime> timers)
    {
        var path = Path.Combine(Environment.CurrentDirectory, FileName);
        var json = JsonSerializer.Serialize(timers, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json + Environment.NewLine);
    }
}
