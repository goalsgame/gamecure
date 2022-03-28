using Gamecure.Core.Common;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Longtail;

public class SetExecutablePermission : IMiddleware<LongtailContext>
{
    public bool ShouldRun(LongtailContext context) => GlobalConfiguration.Platform is Platform.Linux or Platform.Macos;

    public async Task<LongtailContext> OnInvoke(LongtailContext context, ContextDelegate<LongtailContext> next)
    {
        context = await next(context);

        if (context.Failed)
        {
            return context;
        }

        Logger.Info("Set executable permission on longtail.");
        try
        {
            var result = await ProcessRunner.Run("chmod", $"+x {context.LongtailPath}");
            if (!result.Success)
            {
                Logger.Error($"Failed to set executable permissions on longtail at path {context.LongtailPath}");
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to set executable permissions on longtail at path {context.LongtailPath}. Exception was thrown ({e.Message})");
        }
        return context;
    }
}