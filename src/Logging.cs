using System.Runtime.InteropServices;

namespace ProObjLogLite;

public class Logging
{
    public static void PrintLog(Flags flags)
    {
        var time = DateTime.Now.ToString("hh:mm:ss tt");
        var date = DateTime.Today.ToLongDateString();

        var log = "";
        log += Color.Blue("[");
        log += Color.Magenta($"{time}");
        log += Color.Blue(" / ");
        log += Color.Green($"{date}");
        log += Color.Blue("]\nLevel: ");

        switch (flags.Level)
        {
            case "INFORMATION":
            case "INFO":
                log += Color.Green(flags.Level);
                break;

            case "WARNING":
            case "WARN":
                log += Color.Yellow(flags.Level);
                break;

            case "FATAL":
            case "ERROR":
                log += Color.Red(flags.Level);
                break;

            case "DEBUG":
                log += Color.Magenta(flags.Level);
                break;

            default:
                log += Color.Blue(flags.Level);
                break;
        }

        log += Color.Blue("\nMessage: ");
        log += Color.Magenta(flags.Message);

        Console.WriteLine(log);
    }

    public static void SaveLog(Flags flags)
    {
        var time = DateTime.Now.ToString("hh:mm:ss tt");
        var date = DateTime.Today.ToLongDateString();
        var log = $"[{time} {date}]\nLevel: {flags.Level}\nMessage: {flags.Message}\n";

        var pathSep = "/";
        var fileName = DateTime.Now.ToString("d_M_yyyy");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            pathSep = "\\";
        }

        if (!Directory.Exists(flags.Directory))
        {
            Directory.CreateDirectory(flags.Directory);
        }

        File.AppendAllLines($"{flags.Directory}{pathSep}{fileName}.log", log.Split("\n"));
    }
}
