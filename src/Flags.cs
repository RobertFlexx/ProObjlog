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
                    // Actual Help Output is to be implemented.
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
                case "--directory":
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
}
