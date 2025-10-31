namespace ProObjLog.Core;

public static class VersionInfo
{
    public const int VersionMajor = 3;
    public const int VersionMinor = 0;
    public const int VersionPatch = 0;
    public static string VersionString => $"{VersionMajor}.{VersionMinor}.{VersionPatch}";
}
