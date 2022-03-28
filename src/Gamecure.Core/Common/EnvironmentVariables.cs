namespace Gamecure.Core.Common;

public static class EnvironmentVariables
{
    public static string? GetString(string name) => Environment.GetEnvironmentVariable(name);
}