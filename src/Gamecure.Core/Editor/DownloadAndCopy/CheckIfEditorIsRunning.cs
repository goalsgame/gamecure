using System.Diagnostics;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Editor.DownloadAndCopy;

public class CheckIfEditorIsRunning : IMiddleware<DownloadEditorContext>
{
    private readonly string[] _processNames = { "UE4Editor", "UnrealEditor", "GameEditor" };
    public async Task<DownloadEditorContext> OnInvoke(DownloadEditorContext context, ContextDelegate<DownloadEditorContext> next)
    {
        foreach (var name in _processNames)
        {
            var processes = Process.GetProcessesByName(name);
            if (processes.Any())
            {
                foreach (var process in processes)
                {
                    Logger.Trace($"Found a Unreal Editor process running with name {name} and PID {process.Id}");
                }
                return context with { Failed = true, Reason = "Unreal Editor is running, please close before trying to update the editor." };
            }
        }

        return await next(context);
    }
}