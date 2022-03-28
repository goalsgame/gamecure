using Gamecure.Core.Common;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Plastic;

public class SetPlasticCLIPath : IMiddleware<PlasticContext>
{
    public async Task<PlasticContext> OnInvoke(PlasticContext context, ContextDelegate<PlasticContext> next)
    {
        var path = GlobalConfiguration.Platform switch
        {
            Platform.Macos => "/usr/local/bin/cm",
            _ => "cm"
        };

        Logger.Trace($"Set Platic CLI path to: {path}");
        return await next(context with
        {
            PlasticCLIPath = path
        });
    }
}