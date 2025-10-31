using System;
using System.IO;

namespace ProObjLog.Core;

public static class Ansi
{
    public static string Blue(string s)    => $"[94m{s}[0m";
    public static string Green(string s)   => $"[92m{s}[0m";
    public static string Yellow(string s)  => $"[93m{s}[0m";
    public static string Red(string s)     => $"[91m{s}[0m";
    public static string Magenta(string s) => $"[95m{s}[0m";
}

public static class FileHelpers
{
    public static void EnsureDir(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }
}

public static class MonitorHelper
{
    public static void Monitor(LogNode node, Action action, bool exitOnException = false, bool rethrow = false)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            node.LogException(ex);
            if (exitOnException)
                Environment.Exit(1);
            if (rethrow)
                throw;
        }
    }
}
