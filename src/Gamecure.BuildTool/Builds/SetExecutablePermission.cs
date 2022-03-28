using Gamecure.Core;
using Gamecure.Core.Common;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.BuildTool.Builds;

internal class SetExecutablePermission : IMiddleware<BuildContext>
{
    public bool ShouldRun(BuildContext context) => context.Runtime == BuildRuntime.Osx;
    public async Task<BuildContext> OnInvoke(BuildContext context, ContextDelegate<BuildContext> next)
    {
        var executable = Path.Combine(context.MacBundle?.BinaryPath!, "Gamecure.GUI");
        if (GlobalConfiguration.Platform is Platform.Linux or Platform.Macos)
        {
            var result = await ProcessRunner.Run("chmod", $"+x {executable}", GlobalConfiguration.BaseDirectory);
            if (!result.Success)
            {
                Logger.Error($"Failed to set executable permission on {executable} with code {result.ExitCode}");
                Logger.Error($"StdErr: {result.StdErrAsString()}");
                Logger.Error($"StdOut: {result.StdOutAsString()}");
            }
        }
        else
        {
            Logger.Warning("Mac executable built on windows, chmod must be performed manually from Mac or Linux to be able to run Gamecure.");
        }
        return await next(context);

    }
}