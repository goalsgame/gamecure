using Gamecure.Core;
using Gamecure.Core.Common;
using Gamecure.Core.Pipeline;

namespace Gamecure.BuildTool.Builds;

internal class RunBuildCommand : IMiddleware<BuildContext>
{
    public async Task<BuildContext> OnInvoke(BuildContext context, ContextDelegate<BuildContext> next)
    {
        var result = await ProcessRunner.Run("dotnet", context.Arguments, GlobalConfiguration.BaseDirectory, TimeSpan.FromMinutes(5), createNewWindow: true, redirectOutput: false);
        if (!result.Success)
        {
            return context with { Failed = true, Reason = $"Failed to build Gamecure binary with code {result.ExitCode}." };
        }
        return await next(context);
    }
}