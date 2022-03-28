using System.Runtime.InteropServices;
using Gamecure.Core.Common;
using Gamecure.Core.Common.Logging;

namespace Gamecure.Core;

public static class GlobalConfiguration
{
    public static Platform Platform { get; } = GetPlatform();
    public static readonly string BaseDirectory = GetBaseDirectory();
    public static readonly string AppDataDirectory = GetWorkingDirectory();

    public static readonly string DependenciesDirectory = Path.Combine(AppDataDirectory, "deps");
    public static readonly string CacheDirectory = Path.Combine(AppDataDirectory, "cache");
    public static readonly string LogsDirectory = Path.Combine(AppDataDirectory, "logs");
    public static readonly string SessionIdentifier = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
    public static readonly string LogFilePath = Path.Combine(LogsDirectory, $"{SessionIdentifier}.log");

    private static string GetWorkingDirectory()
    {
        //TODO: Add check if its running with dotnet run or in VS
        var path = GetAppDataPath();
        if (!string.IsNullOrWhiteSpace(path))
        {
            return Path.Combine(path, "GOALS", "Gamecure");
        }
        // If we can't determine a path, just use the path where the executable is. (this will unfortunately fail on MacOS)
        return BaseDirectory;

        static string GetAppDataPath()
        {
            var workdingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrWhiteSpace(workdingDirectory))
            {
                var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (!string.IsNullOrWhiteSpace(userProfile))
                {
                    workdingDirectory = Path.Combine(userProfile, ".config");
                }
            }
            return workdingDirectory;
        }
    }
    private static string GetBaseDirectory()
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        // NOTE(Jens): If the app is compiled in Release it wont try to find the root folder
#if DEBUG
        const string SolutionFilename = "Gamecure.sln";

        return FindParentWithFile(SolutionFilename, baseDirectory, 7)
            ?? baseDirectory;

        static string? FindParentWithFile(string filename, string? path, int steps)
        {
            if (steps == 0 || path == null)
            {
                return null;
            }
            return File.Exists(Path.Combine(path, filename))
                ? path
                : FindParentWithFile(filename, Directory.GetParent(path)?.FullName, steps - 1);
        }
#else
    return baseDirectory;
#endif
    }

    public static void Init()
    {
        Directory.CreateDirectory(DependenciesDirectory);
        Directory.CreateDirectory(CacheDirectory);
    }

    private static Platform GetPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Platform.Windows;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Platform.Linux;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Platform.Macos;
        }
        Logger.Error("The current platform is not supported.");
        throw new NotSupportedException("Platform is not supported.");
    }
}