using System.Diagnostics;
using Gamecure.Core.Common.Logging;

namespace Gamecure.Core.Common;

public static class ApplicationLauncher
{
    public static async Task<bool> LaunchUnrealEditor(string? workspace, TimeSpan waitTime = default)
    {
        if (workspace == null)
        {
            return false;
        }

        // NOTE(Jens): These are the known editor binaries, this is also the priority of which one will be started.
        var unrealEditorPaths = new[]
        {
            // NOTE(Jens): This is a workaround to support the current version of the GameEditor. Passing game.uproject to the binary does not work.
            GetEditorPath(workspace, "Game", "GameEditor"),
            GetEditorPath(workspace, "Engine", "UnrealEditor"),
            GetEditorPath(workspace, "Engine", "UE4Editor"),
        };

        foreach (var unrealEditorPath in unrealEditorPaths)
        {
            if (!File.Exists(unrealEditorPath))
            {
                Logger.Trace($"No binary found at {unrealEditorPath}");
                continue;
            }

            var projectPath = Path.Combine(workspace, "Game", "Game.uproject");
            var quotedArguments = $"\"{projectPath}\"";
            Logger.Trace($"Trying to launch unreal editor from path {unrealEditorPath} and arguments {quotedArguments}");

            using var process = Process.Start(new ProcessStartInfo(unrealEditorPath, quotedArguments)
            {
                WorkingDirectory = workspace
            });

            if (process == null)
            {
                Logger.Error($"Failed to start Unreal Editor from path {unrealEditorPath}");
                return false;
            }
            await Task.Delay(waitTime);
            if (process.HasExited)
            {
                Logger.Error($"Unreal Editor was closed immediately with code {process.ExitCode}");
                return false;
            }
            return true;
        }
        Logger.Error("Could not find any editor binary.");
        return false;

        static string GetEditorPath(string workspace, string subDir, string processName) =>
            GlobalConfiguration.Platform switch
            {
                Platform.Windows => Path.Combine(workspace, subDir, "Binaries", "Win64", $"{processName}.exe"),
                Platform.Linux => Path.Combine(workspace, subDir, "Binaries", "Linux", processName),
                Platform.Macos => Path.Combine(workspace, subDir, "Binaries", "Mac", $"{processName}.app", "Contents", "MacOS", processName),
                _ => throw new NotSupportedException("platform not supported.")
            };
    }

    public static async Task<bool> LaunchGluon(string? workspace, TimeSpan waitTime = default)
    {
        if (workspace == null)
        {
            Logger.Warning("Failed to start Gluon, workspace is null.");
            return false;
        }

        var processPath = GlobalConfiguration.Platform switch
        {
            Platform.Macos => "/Applications/Gluon.app/Contents/MacOS/Gluon",
            _ => "gluon"
        };

        using var process = Process.Start(new ProcessStartInfo(processPath)
        {
            WorkingDirectory = workspace
        });

        if (process == null)
        {
            Logger.Warning("Failed to start Gluon, process start returned null.");
            return false;
        }

        // NOTE(Jens): Wait for a short time before we check HasExited. 
        await Task.Delay(waitTime);
        if (process.HasExited)
        {
            Logger.Error($"Gluon was closed immediately");
            return false;
        }
        return true;
    }

    public static async Task<bool> LaunchBrowser(string url, Querystring? querystring = null, TimeSpan waitTime = default)
    {
        var path = querystring != null ? $"{url}?{querystring}" : url;

        using var process = GlobalConfiguration.Platform switch
        {
            Platform.Windows => Start("cmd", $"/c start {path.Replace("&", "^&")}"),
            Platform.Linux => Start("xdg-open", path),
            Platform.Macos => Start("open", path),
            _ => throw new NotSupportedException("Platform not supported.")
        };

        await Task.Delay(waitTime);
        if (process.HasExited)
        {
            Logger.Error($"Browser was closed immediately");
            return false;
        }
        return true;


        static Process Start(string command, string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo(command, arguments)
                {
                    CreateNoWindow = true
                }
            };
            process.Start();
            return process;
        }
    }
}