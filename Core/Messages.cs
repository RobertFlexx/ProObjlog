using System;

namespace ProObjLog.Core;

public sealed class DebugMessage : LogMessage
{
    public DebugMessage(string message) : base(message)
    {
        Level = "DEBUG";
    }

    public override string Colorize(string text) => Ansi.Blue(text);
}

public sealed class InfoMessage : LogMessage
{
    public InfoMessage(string message) : base(message)
    {
        Level = "INFO";
    }

    public override string Colorize(string text) => Ansi.Green(text);
}

public sealed class WarnMessage : LogMessage
{
    public WarnMessage(string message) : base(message)
    {
        Level = "WARN";
    }

    public override string Colorize(string text) => Ansi.Yellow(text);
}

public sealed class ErrorMessage : LogMessage
{
    public ErrorMessage(string message) : base(message)
    {
        Level = "ERROR";
    }

    public override string Colorize(string text) => Ansi.Red(text);
}

public sealed class FatalMessage : LogMessage
{
    public FatalMessage(string message) : base(message)
    {
        Level = "FATAL";
    }

    public override string Colorize(string text) => Ansi.Magenta(text);
}

public sealed class ExceptionMessage : LogMessage
{
    public Exception ExceptionObj { get; }

    public ExceptionMessage(Exception ex) : base(BuildMessage(ex))
    {
        ExceptionObj = ex;
        Level = $"EXCEPTION ({ex.GetType().Name})";
    }

    private static string BuildMessage(Exception ex)
    {
        var where = ex.StackTrace?.Split('\n', '\r')[0]?.Trim();
        if (string.IsNullOrEmpty(where))
            where = "N/A";
        return $"{ex.Message} ({where})";
    }

    public override string Colorize(string text) => Ansi.Red(text);
}
