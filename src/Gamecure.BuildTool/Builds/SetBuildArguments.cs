using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.BuildTool.Builds;

internal class SetBuildArguments : IMiddleware<BuildContext>
{
    public async Task<BuildContext> OnInvoke(BuildContext context, ContextDelegate<BuildContext> next)
    {
        List<string> argsList = new() { "publish src/Gamecure.GUI" };

        var runtime = context.Runtime switch
        {
            BuildRuntime.Win => "win-x64",
            BuildRuntime.Osx => "osx-x64",
            BuildRuntime.Linux => "linux-x64",
            _ => null
        };

        if (runtime == null)
        {
            return context with { Failed = true, Reason = $"Unsupported runtime: {context.Runtime}" };
        }

        argsList.Add($"-r {runtime}");

        if (context.OutputFolder != null)
        {
            Logger.Trace($"OutputPath: {context.OutputFolder}");
            argsList.Add($"-o {context.OutputFolder}");
        }

        if (context.Config != null)
        {
            argsList.Add($"-c {context.Config}");
        }
        argsList.Add("--self-contained true");
        argsList.Add("-p:DebugType=none");
        argsList.Add("-p:PublishSingleFile=true");

        var arguments = string.Join(' ', argsList);

        Logger.Trace($"Build arguments: {arguments}");

        return await next(context with
        {
            Arguments = arguments
        });
    }
}