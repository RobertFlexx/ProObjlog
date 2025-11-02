using System.Diagnostics.Tracing;
using ProObjLog.Core;

namespace ProObjLog;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length < 3)
        { 
            Console.WriteLine("Not enough args provided. At least 3 required.\nUsage: ProObjLog <log|level> <LEVEL> <message...>");
            return;
        }

        var level = args[1].ToUpperInvariant();
        var msg = args.Length > 2 ? string.Join(' ', args[2..]) : string.Empty;
        var conPrint = true;

        if (args[0].Equals("log", StringComparison.CurrentCultureIgnoreCase)) {
            conPrint = false;
        } else if (args[0].Equals("console", StringComparison.CurrentCultureIgnoreCase)) {
            conPrint = true;
        } else {
            Console.WriteLine(Ansi.Red("First value must be log or print."));
            Environment.Exit(1);
        }

        var logger = new LogNode(name: "prObjLog", printToConsole: conPrint);
        LogMessage toLog = level switch
        {
            "DEBUG" => new DebugMessage(msg),
            "INFO"  => new InfoMessage(msg),
            "WARN"  => new WarnMessage(msg),
            "ERROR" => new ErrorMessage(msg),
            "FATAL" => new FatalMessage(msg),
            _       => new InfoMessage($"[{level}] {msg}")
        };

        logger.Log(toLog);
    }
}
