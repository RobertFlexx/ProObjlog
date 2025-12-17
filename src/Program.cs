namespace ProObjLogLite;

public class Program
{
    public static void Main(string[] args)
    {
        Flags flags;
        try
        {
            flags = Flags.Parse(args.ToList());
        }
        catch (Exception e)
        {
            Console.WriteLine(Color.Red(e.Message));
            Environment.Exit(1);
            return;
        }

        if (!flags.NoPrint)
            Logging.PrintLog(flags);

        try
        {
            Logging.SaveLog(flags);
        }
        catch (Exception e)
        {
            Console.WriteLine(Color.Red(e.Message));
            Environment.Exit(2);
        }
    }
}
