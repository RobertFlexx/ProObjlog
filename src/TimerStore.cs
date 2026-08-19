using System.Text.Json;

namespace ProObjLogLite;

public static class TimerStore
{
    private const string FileName = ".proobjloglite_timers.json";
    private static readonly object StoreGate = new();

    public static void Start(string name, DateTime startedAt)
    {
        lock (StoreGate)
        {
            var path = Path.Combine(Environment.CurrentDirectory, FileName);
            using var fileLock = FileLock.Acquire(path + ".lock");
            var timers = Load(path);
            timers[name] = startedAt;
            Save(path, timers);
        }
    }

    public static TimeSpan Stop(string name, DateTime endedAt)
    {
        lock (StoreGate)
        {
            var path = Path.Combine(Environment.CurrentDirectory, FileName);
            using var fileLock = FileLock.Acquire(path + ".lock");
            var timers = Load(path);
            if (!timers.TryGetValue(name, out var startedAt))
                throw new InvalidOperationException($"Timer '{name}' has not been started.");

            timers.Remove(name);
            Save(path, timers);
            return endedAt - startedAt;
        }
    }

    private static Dictionary<string, DateTime> Load(string path)
    {
        if (!File.Exists(path))
            return new Dictionary<string, DateTime>(StringComparer.Ordinal);

        var json = File.ReadAllText(path);
        var parsed = JsonSerializer.Deserialize<Dictionary<string, DateTime>>(json, JsonContext.Default.DictionaryStringDateTime);
        return parsed ?? new Dictionary<string, DateTime>(StringComparer.Ordinal);
    }

    private static void Save(string path, Dictionary<string, DateTime> timers)
    {
        var json = JsonSerializer.Serialize(timers, JsonContext.Default.DictionaryStringDateTime);
        File.WriteAllText(path, json + Environment.NewLine);
    }
}
