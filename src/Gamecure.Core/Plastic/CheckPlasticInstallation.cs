using Gamecure.Core.Common;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Plastic;

public class CheckPlasticInstallation : IMiddleware<PlasticContext>
{
    //NOTE(Jens): Only run this once per application instance, even if the workspace changes.
    private static string? _version;
    public bool ShouldRun(PlasticContext context) => _version == null;

    public async Task<PlasticContext> OnInvoke(PlasticContext context, ContextDelegate<PlasticContext> next)
    {
        var result = await ProcessRunner.Run(context.PlasticCLIPath, "version", GlobalConfiguration.BaseDirectory);
        if (!result.Success)
        {
            return context with { Failed = true, Reason = $"Failed to run command 'cm version' with error: {result.StdErrAsString()}. Make sure you've installed PlasticSCM." };
        }

        if (result.StdOut.Length == 1)
        {
            _version = result.StdOut[0];
            Logger.Trace($"Plastic version: {_version}");
        }
        else
        {
            _version = "n/a";
        }

        return await next(context with { Version = _version });
    }
}