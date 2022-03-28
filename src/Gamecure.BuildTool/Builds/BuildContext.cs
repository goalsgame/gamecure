
using Gamecure.Core.Pipeline;

namespace Gamecure.BuildTool.Builds;

internal enum BuildRuntime
{
    Win,
    Linux,
    Osx
}

internal enum BuildPackage
{
    Application,
    Installer
}

internal record MacBundleConfig(string AppPath, string ContentPath, string BinaryPath, string ResourcesPath);
internal record BuildContext : Context
{
    public string? Config { get; init; }
    public BuildRuntime? Runtime { get; init; }
    public string? OutputFolder { get; init; }
    public string Arguments { get; init; } = string.Empty;
    public MacBundleConfig? MacBundle { get; init; }
    public string? ZipFile { get; init; }
    public string? ConfigFile { get; init; }
    public BuildPackage Package { get; init; }
}