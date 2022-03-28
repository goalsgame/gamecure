using System.Diagnostics;
using Gamecure.Core.Common.Logging;

namespace Gamecure.Core.Common;

public class ProcessRunner
{
    public static ValueTask<ProcessResult> Run(string command, string arguments) => Run(command, arguments, GlobalConfiguration.AppDataDirectory, TimeSpan.FromSeconds(30));
    public static ValueTask<ProcessResult> Run(string command, string arguments, string workingDirectory) => Run(command, arguments, workingDirectory, TimeSpan.FromSeconds(30));
    public static async ValueTask<ProcessResult> Run(string command, string arguments, string workingDirectory, TimeSpan timeout, KeyValuePair<string, string>[]? environmentVariables = null, bool createNewWindow = false, bool redirectOutput = true)
    {
        Logger.Trace($"Run command: {command} {arguments}");
        var startInfo = new ProcessStartInfo(command, arguments)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = redirectOutput,
            RedirectStandardError = redirectOutput,
            CreateNoWindow = !createNewWindow
        };

        foreach (var (key, value) in environmentVariables ?? Array.Empty<KeyValuePair<string, string>>())
        {
            startInfo.Environment.Add(key, value);
        }

        Process process;
        try
        {
            process = new Process { StartInfo = startInfo };
            if (!process.Start())
            {
                return new ProcessResult
                {
                    Success = false,
                    Reason = $"Failed to start command {command}"
                };
            }
        }
        catch
        {
            return new ProcessResult
            {
                Success = false,
                Reason = "Failed because of exception"
            };
        }
        List<string> stdout = new();
        List<string> stderr = new();
        if (redirectOutput)
        {
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();


            process.OutputDataReceived += (_, args) =>
            {
                if (args.Data != null)
                {
                    stdout.Add(args.Data);
                }
            };
            process.ErrorDataReceived += (_, args) =>
            {
                if (args.Data != null)
                {
                    stderr.Add(args.Data);
                }
            };
        }

        CancellationTokenSource timeoutSource = new();
        timeoutSource.CancelAfter(timeout);
        try
        {
            await process.WaitForExitAsync(timeoutSource.Token);
        }
        catch (Exception e) when (e is OperationCanceledException or TaskCanceledException)
        {
            return new ProcessResult
            {
                Success = false,
                Reason = $"Timeout ({timeout}) reached for command {command} ({e.GetType().Name})",
                StdErr = stderr.ToArray(),
                StdOut = stdout.ToArray()
            };
        }
        finally
        {
            timeoutSource.Dispose();
        }

        if (!process.HasExited)
        {
            process.Kill(true);
        }
        return new ProcessResult
        {
            ExitCode = process.ExitCode,
            Success = process.ExitCode == 0,
            StdErr = stderr.ToArray(),
            StdOut = stdout.ToArray()
        };
    }
}