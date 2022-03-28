using Gamecure.Core.Common;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Longtail;

public class SetLongtailPath : IMiddleware<LongtailContext>
{
    public async Task<LongtailContext> OnInvoke(LongtailContext context, ContextDelegate<LongtailContext> next)
    {
        var depsDirectory = GlobalConfiguration.DependenciesDirectory;
        var path = GlobalConfiguration.Platform switch
        {
            Platform.Windows => Path.Combine(depsDirectory, "dist-win32-x64", "longtail.exe"),
            Platform.Linux => Path.Combine(depsDirectory, "dist-linux-x64", "longtail"),
            Platform.Macos => Path.Combine(depsDirectory, "dist-macos-x64", "longtail"),
            _ => throw new ArgumentOutOfRangeException()
        };
        return await next(context with { LongtailPath = path });
    }
}