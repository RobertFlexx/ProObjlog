namespace ProObjLogLite;

public class Flags
{
    public required string Level { get; set; }
    public required string Message { get; set; }
    public required string Directory { get; set; }
    public required bool NoPrint { get; set; }

    public static Flags Parse(List<string> args)
    {
        var flags = new Flags
        {
            Level = "INFO",
            Message = "No message provided",
            Directory = "logs",
            NoPrint = false
        };

        foreach (var arg in args.Where(arg => arg.StartsWith('-')))
        {
            switch (arg)
            {
                case "--help":
                case "-h":
                    PrintHelp();
                    Environment.Exit(0);
                    break;

                case "-l":
                case "--level":
                    if (args.IndexOf(arg) + 2 > args.Count)
                        throw new Exception("No level provided after flag");

                    flags.Level = args[args.IndexOf(arg) + 1].ToUpper();
                    break;

                case "-m":
                case "--message":
                    if (args.IndexOf(arg) + 2 > args.Count)
                        throw new Exception("No message provided after flag");

                    flags.Message = args[args.IndexOf(arg) + 1];
                    break;

                case "-d":
                case "--dir":
                    if (args.IndexOf(arg) + 2 > args.Count)
                        throw new Exception("No directory provided after flag");

                    flags.Directory = args[args.IndexOf(arg) + 1];
                    break;

                case "-n":
                case "--noprint":
                    flags.NoPrint = true;
                    break;

                default:
                    throw new Exception($"Unknown flag {arg}.");
            }
        }

        return flags;
    }

    private static void PrintHelp()
    {
        Console.WriteLine(Color.Blue("USAGE: ProObjLogLite <OPTIONS>"));
        Console.WriteLine(Color.Green("FLAGS:"));
        
        Console.Write(Color.Magenta("\t-d | --dir"));
        Console.WriteLine(Color.Yellow("\t=> Sets the directory where the log is to be stored."));

        Console.Write(Color.Magenta("\t-l | --level"));
        Console.WriteLine(Color.Yellow("\t=> Sets the level of the log."));

        Console.Write(Color.Magenta("\t-m | --message"));
        Console.WriteLine(Color.Yellow("\t=> Sets the message of the log."));

        Console.Write(Color.Magenta("\t-n | --noprint"));
        Console.WriteLine(Color.Yellow("\t=> Disables printing to console for the log and only saves it."));
    }
}
