using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Plastic;

public class CheckIfConfigurationIsRequired : IMiddleware<PlasticContext>
{
    public bool ShouldRun(PlasticContext context) => context.RequiresConfiguration == false;
    public async Task<PlasticContext> OnInvoke(PlasticContext context, ContextDelegate<PlasticContext> next)
    {
        foreach (var include in context.Config.Includes)
        {
            //NOTE(Jens): needs to verify that this is a good way to do this. maybe we can check the configuration with a plastic command?
            var fullPath = Path.Combine(context.Workspace, include);
            var exists = File.Exists(fullPath) || Directory.Exists(fullPath);
            if (!exists)
            {
                return await next(context with { RequiresConfiguration = true });
            }
        }
        return await next(context);
    }
}