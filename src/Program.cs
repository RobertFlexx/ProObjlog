namespace ProObjLogLite;

public static class Program
{
    public static int Main(string[] args)
    {
        return MainAsync(args).GetAwaiter().GetResult();
    }

    private static async Task<int> MainAsync(string[] args)
    {
        Flags flags;
        try
        {
            flags = Flags.Parse(args);
        }
        catch (Exception e)
        {
            Console.WriteLine(Color.Red($"Argument error: {e.Message}"));
            return 1;
        }

        Color.Enabled = !(flags.NoColor || Console.IsOutputRedirected);

        if (flags.HelpRequested)
        {
            Flags.PrintHelp();
            return 0;
        }

        if (flags.VersionRequested)
        {
            Console.WriteLine($"ProObjLogLite {GetVersion()}");
            return 0;
        }

        if (flags.ListLevelsRequested)
        {
            Console.WriteLine(string.Join(", ", Logging.KnownLevels));
            return 0;
        }

        if (flags.InitConfigRequested)
        {
            CreateConfigTemplate();
            return 0;
        }

        try
        {
            LogEngine.Apply(flags);

            if (flags.TimerStart)
            {
                StartTimer(flags);
                return 0;
            }

            if (flags.TimerStop)
                StopTimer(flags);
        }
        catch (Exception e)
        {
            Console.WriteLine(Color.Red($"Timer error: {e.Message}"));
            return 1;
        }

        if (LogEngine.ShouldSkip(flags))
        {
            if (!flags.NoPrint)
                Console.WriteLine(Color.Yellow("Skipped by --when expression."));
            return 0;
        }

        if (!Logging.ShouldLog(flags))
        {
            if (!flags.NoPrint)
                Console.WriteLine(Color.Yellow($"Skipped: level {flags.Level} is below min-level {flags.MinLevel}."));
            return 0;
        }

        if (!Logging.PassesSampling(flags))
        {
            if (!flags.NoPrint)
                Console.WriteLine(Color.Yellow($"Skipped by sample rate {flags.SampleRate:0.###}."));
            return 0;
        }

        var entry = Logging.CreateEntry(flags);
        entry = Logging.FinalizeForDisplay(entry, flags);

        if (!flags.NoPrint)
            Logging.PrintLog(entry, flags);

        if (flags.StdoutOnly || flags.DryRun)
            return 0;

        string outputPath = "";
        try
        {
            for (var i = 0; i < flags.Count; i++)
            {
                var current = Logging.CreateEntry(flags);
                if (!flags.AsyncWrite)
                    outputPath = Logging.SaveLog(current, flags);
                else if (flags.FireAndForget)
                    _ = Logging.EnqueueLogAsync(current, flags, waitForCompletion: false);
                else
                    outputPath = await Logging.EnqueueLogAsync(current, flags, waitForCompletion: true);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(Color.Red($"Write error: {e.Message}"));
            await Logging.FlushAsync();
            return 2;
        }

        await Logging.FlushAsync();

        if (!flags.NoPrint)
        {
            if (flags.FireAndForget)
                Console.WriteLine(Color.Green("Queued async log write."));
            else
                Console.WriteLine(Color.Green($"Saved log to {outputPath}{(flags.AsyncWrite ? " (async)" : "")}"));
        }

        return 0;
    }

    private static string GetVersion()
    {
        return typeof(Program).Assembly.GetName().Version?.ToString() ?? "0.0.0";
    }

    private static void CreateConfigTemplate()
    {
        const string fileName = "proobjloglite.json";
        var path = Path.Combine(Environment.CurrentDirectory, fileName);

        if (File.Exists(path))
        {
            Console.WriteLine(Color.Yellow($"Config already exists at {path}"));
            return;
        }

        var template = """
        {
          "directory": "logs",
          "level": "INFO",
          "minLevel": "TRACE",
          "jsonOutput": false,
          "source": "default",
          "utc": false,
          "timestampFormat": "yyyy-MM-ddTHH:mm:ss.fffK",
          "maxFiles": 7
        }
        """;

        File.WriteAllText(path, template + Environment.NewLine);
        Console.WriteLine(Color.Green($"Created config template at {path}"));
    }

    private static void StartTimer(Flags flags)
    {
        var now = flags.Utc ? DateTime.UtcNow : DateTime.Now;
        TimerStore.Start(flags.TimerName!, now);
        Console.WriteLine(Color.Green($"Started timer '{flags.TimerName}' at {now:O}"));
    }

    private static void StopTimer(Flags flags)
    {
        var now = flags.Utc ? DateTime.UtcNow : DateTime.Now;
        var elapsed = TimerStore.Stop(flags.TimerName!, now);
        flags.Context["timer"] = flags.TimerName!;
        flags.Context["elapsed_ms"] = ((long)elapsed.TotalMilliseconds).ToString();
        if (string.IsNullOrWhiteSpace(flags.Message) || flags.Message == "No message provided")
            flags.Message = $"Timer '{flags.TimerName}' completed in {elapsed.TotalMilliseconds:F0}ms";
    }
}
