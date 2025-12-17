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
            return;
        }

        if (!flags.NoPrint)
            Console.WriteLine(Logging.MakeLog(flags));

        try
        {
            Logging.SaveLog(flags);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
