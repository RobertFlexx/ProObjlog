namespace ProObjLog.Core;

public static class VersionInfo
{
    public const int VersionMajor = 3;
    public const int VersionMinor = 1;
    public const int VersionPatch = 1;
    public static string VersionString => $"{VersionMajor}.{VersionMinor}.{VersionPatch}";
}
