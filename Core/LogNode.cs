using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProObjLog.Core;

public sealed class LogNode
{
    private readonly List<LogMessage> _messages;
    private readonly int _maxMessagesInMemory;
    private readonly int _maxMessagesInFile;

    public string Name { get; private set; }
    public string? LogFilePath { get; private set; }
    public bool PrintToConsole { get; set; }
    public bool Enabled { get; set; } = true;

    public static LogNode? Global { get; private set; }

    public LogNode(
        string name,
        string? logFile = null,
        bool printToConsole = true,
        int maxMessagesInMemory = 500,
        int maxMessagesInFile = 1000)
    {
        Name = name;
        PrintToConsole = printToConsole;
        _maxMessagesInMemory = maxMessagesInMemory;
        _maxMessagesInFile = maxMessagesInFile;
        _messages = new List<LogMessage>(_maxMessagesInMemory);

        if (string.IsNullOrWhiteSpace(logFile))
        {
            var logsDir = Path.Combine(AppContext.BaseDirectory, "logs");
            FileHelpers.EnsureDir(logsDir);
            var today = DateTime.Now.ToString("yyyy-MM-dd");
            logFile = Path.Combine(logsDir, $"{today}.log");
        }

        LogFilePath = logFile;

        if (Global is null)
            Global = this;
    }

    public void Log(LogMessage message, bool forcePrint = false)
    {
        if (!Enabled) return;

        if (_messages.Count >= _maxMessagesInMemory)
            _messages.RemoveAt(0);
        _messages.Add(message);

        if (LogFilePath is not null)
            AppendToFile(message);

        if (PrintToConsole || forcePrint)
        {
            var line = $"[{Name}] {message.Format()}";
            Console.WriteLine(message.Colorize(line));
        }
    }

    private void AppendToFile(LogMessage message)
    {
        if (LogFilePath is null) return;

        List<string> lines = new List<string>();
        if (File.Exists(LogFilePath))
            lines = File.ReadAllLines(LogFilePath).ToList();

        if (lines.Count >= _maxMessagesInFile)
        {
            lines = lines.Skip(Math.Max(0, lines.Count - (_maxMessagesInFile - 1))).ToList();
        }

        lines.Add($"[{Name}] {message.Format()}");
        File.WriteAllLines(LogFilePath, lines);
    }

    public void Rename(string newName)
    {
        Name = newName;
    }

    public IReadOnlyList<LogMessage> GetMessages() => _messages.AsReadOnly();

    public void LogException(Exception ex)
    {
        Log(new ExceptionMessage(ex), forcePrint: true);
    }
}
