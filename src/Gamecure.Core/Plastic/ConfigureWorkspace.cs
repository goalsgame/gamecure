using Gamecure.Core.Common;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Plastic;

public class ConfigureWorkspace : IMiddleware<PlasticContext>
{
    public bool ShouldRun(PlasticContext context) => context.RequiresConfiguration;
    public async Task<PlasticContext> OnInvoke(PlasticContext context, ContextDelegate<PlasticContext> next)
    {
        var config = context.Config;

        var includes = string.Join(' ', config.Includes.Select(i => $"+/{i}"));
        var excludes = string.Join(' ', config.Excludes.Select(e => $"-/{e}"));
        Logger.Trace("Configure partial workspace");
        Logger.Trace($"Includes: {includes}");
        Logger.Trace($"Excludes: {excludes}");
        var result = await ProcessRunner.Run(context.PlasticCLIPath, $"partial configure {includes} {excludes} --ignorefailed", context.Workspace, TimeSpan.FromMinutes(30));
        if (result.Success)
        {
            return await next(context);
        }
        Logger.Error("cm partial configure failed.");
        Logger.Error($"StdOut: {result.StdOutAsString()}");
        Logger.Error($"StdErr: {result.StdErrAsString()}");
        return context with { Failed = true, Reason = "cm partial configure failed." };
    }
}
