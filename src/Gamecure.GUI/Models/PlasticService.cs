using System.Threading.Tasks;
using Gamecure.Core.Pipeline;
using Gamecure.Core.Plastic;

namespace Gamecure.GUI.Models;

internal class PlasticService : IPlasticService
{
    private readonly IConfigurationService _configurationService;

    public PlasticService(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    public async Task<PlasticContext> Run()
    {
        var config = await _configurationService.GetUserConfig();
        return await Run(config?.Workspace);
    }

    public async Task<PlasticContext> Run(string? workspace)
    {
        var appConfig = await _configurationService.GetAppConfig();
        var plasticPipeline = new PipelineBuilder<PlasticContext>()
            .With<SetPlasticCLIPath>()
            .With<CheckPlasticInstallation>()
            .With<CreateWorkspace>()
            .With<CheckIfConfigurationIsRequired>()
            .With<ConfigureWorkspace>()
            .With<ValidatePlasticWorkspace>()
            .With<PlasticChangesets>()
            .Build()
            .Invoke(new PlasticContext(appConfig.Plastic)
            {
                Workspace = workspace ?? string.Empty
            });

        return await plasticPipeline;
    }
}
