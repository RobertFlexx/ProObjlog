using ProObjLog.Core;

namespace ProObjLog;

public static class Program
{
    public static int Main(string[] args)
    {
        var logger = new LogNode(name: "prObjLog", printToConsole: true);

        if (args.Length == 0)
        {
            logger.Log(new InfoMessage("no message provided"));
            Console.WriteLine("usage: ProObjLog <LEVEL> <message...>");
            return 1;
        }

        var level = args[0].ToUpperInvariant();
        var msg = args.Length > 1 ? string.Join(' ', args[1..]) : string.Empty;

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
        return 0;
    }
}
