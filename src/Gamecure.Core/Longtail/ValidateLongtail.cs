using Gamecure.Core.Common;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Longtail;

public class ValidateLongtail : IMiddleware<LongtailContext>
{
    public async Task<LongtailContext> OnInvoke(LongtailContext context, ContextDelegate<LongtailContext> next)
    {
        if (!File.Exists(context.LongtailPath))
        {
            Logger.Trace("Longtail not found");
            context = await next(context);
            if (context.Failed)
            {
                return context;
            }
        }

        if (!File.Exists(context.LongtailPath))
        {
            Logger.Error("Longtail not found");
            return context with { Failed = true, Reason = $"Longtail was not found in path {context.LongtailPath}" };
        }
        Logger.Trace("Longtail found");
        try
        {
            Logger.Trace("Running longtail --help");
            var result = await ProcessRunner.Run(context.LongtailPath, "--help");
            if (!result.Success)
            {
                return context with { Failed = true, Reason = $"Longtail process failed with code {result.ExitCode}" };
            }
        }
        catch (Exception)
        {
            return context with { Failed = true, Reason = "Exception was thrown when trying to run longtail." };
        }

        Logger.Trace("Longtail validation successful");
        return context;

    }
}