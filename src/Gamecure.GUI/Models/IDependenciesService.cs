using System.Threading.Tasks;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Longtail;
using Gamecure.Core.Pipeline;

namespace Gamecure.GUI.Models;

public interface IDependenciesService
{
    Task<LongtailContext> Run(bool reset = false);
}

internal class DependenciesService : IDependenciesService
{
    private readonly IConfigurationService _configuration;

    public DependenciesService(IConfigurationService configuration)
    {
        _configuration = configuration;
    }
    public async Task<LongtailContext> Run(bool reset)
    {
        Logger.Trace("Running dependencies pipeline");
        var config = await _configuration.GetAppConfig();

        var longtailPipeline = new PipelineBuilder<LongtailContext>()
            .With<SetLongtailPath>()
            .With<DeleteLongtail>()
            .With<ValidateLongtailChecksum>()
            .With<ValidateLongtail>()
            .With<SetExecutablePermission>()
            .With<GetLongtailVersion>()
            .With<DownloadLongtail>()
            .Build();
        var result = await longtailPipeline
            .Invoke(new LongtailContext(config.Longtail.Version)
            {
                Reset = reset
            });

        if (result.Failed)
        {
            Logger.Error($"Dependencies pipeline failed with error: {result.Reason}");
        }
        return result;
    }
}