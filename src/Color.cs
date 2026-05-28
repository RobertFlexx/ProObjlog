namespace ProObjLogLite;

public static class Color
{
    public static bool Enabled { get; set; } = true;

    public static string Blue(string s)    => Wrap("\u001b[94m", s);
    public static string Green(string s)   => Wrap("\u001b[92m", s);
    public static string Yellow(string s)  => Wrap("\u001b[93m", s);
    public static string Red(string s)     => Wrap("\u001b[91m", s);
    public static string Magenta(string s) => Wrap("\u001b[95m", s);

    private static string Wrap(string colorCode, string value)
    {
        if (!Enabled)
            return value;

        return $"{colorCode}{value}\u001b[0m";
    }
}
