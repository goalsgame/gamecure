using System.Windows.Input;
using Gamecure.GUI.Common;
using Gamecure.GUI.Models;
using Gamecure.GUI.ViewModels.FirstTimeSetup;
using ReactiveUI;
using Splat;

namespace Gamecure.GUI.ViewModels.Config;

internal class ConfigViewModel : ViewModelBase
{
    public IAsyncTask<UserConfig> LoadConfigTask { get; }
    public ICommand SaveConfig { get; }
    public WorkspaceSetupViewModel Workspace { get; }
    public GoogleSignInViewModel Google { get; }
    public DependenciesViewModel Dependencies { get; }
    public SetupUnrealEngineViewModel Setup { get; }

    private bool _deletePreviousEditor = false;
    public bool DeletePreviousEditor
    {
        get => _deletePreviousEditor;
        set => SetProperty(ref _deletePreviousEditor, value);
    }

    public ConfigViewModel()
    : this(
        Locator.Current.GetRequiredService<IConfigurationService>(),
        Locator.Current.GetRequiredService<IPlasticService>(),
        Locator.Current.GetRequiredService<IGoogleAuthService>(),
        Locator.Current.GetRequiredService<IDependenciesService>(),
        Locator.Current.GetRequiredService<IEditorService>()
        )
    {
    }

    public ConfigViewModel(IConfigurationService configurationService, IPlasticService plasticService, IGoogleAuthService googleAuthService, IDependenciesService dependenciesService, IEditorService editorService)
    {
        Workspace = new WorkspaceSetupViewModel(plasticService);
        Google = new GoogleSignInViewModel(googleAuthService);
        Dependencies = new DependenciesViewModel(dependenciesService);
        Setup = new SetupUnrealEngineViewModel(editorService);

        LoadConfigTask = new AsyncTaskModel<UserConfig>(async () =>
        {
            var config = await configurationService.GetUserConfig();

            Workspace.Folder = config?.Workspace;
            Google.Secret = config?.ClientSecret;
            Dependencies.LongtailPath = config?.LongtailPath;

            return config;
        }, startImmediately: true);

        SaveConfig = ReactiveCommand.CreateFromTask(async () =>
        {
            var config = await configurationService.GetUserConfig() ?? new UserConfig();
            await configurationService.SaveUserConfig(config with
            {
                ClientSecret = Google.Secret ?? string.Empty,
                LongtailPath = Dependencies.LongtailPath ?? string.Empty,
                Workspace = Workspace.Folder ?? string.Empty,
                DeletePreviousEditor = DeletePreviousEditor
            });
        });
    }
}