using System;
using System.Windows.Input;
using Gamecure.GUI.Models;
using ReactiveUI;
using Splat;

namespace Gamecure.GUI.ViewModels.FirstTimeSetup;

public class FirstTimeSetupViewModel : ViewModelBase
{
    public WorkspaceSetupViewModel Workspace { get; }
    public SetupUnrealEngineViewModel Setup { get; }
    public GoogleSignInViewModel Google { get; }
    public DependenciesViewModel Dependencies { get; }
    public ICommand CreateWorkspace { get; }
    public ICommand SetupUnrealEngine { get; }
    public ICommand SignInToGoogle { get; }
    public ICommand DownloadDependencies { get; }
    public ICommand SaveAndContinue { get; }
    private SetupSteps _current = SetupSteps.Workspace;
    public bool IsWorkspaceStep => _current == SetupSteps.Workspace;
    public bool IsGoogleStep => _current == SetupSteps.Google;
    public bool IsDependenciesStep => _current == SetupSteps.Dependencies;
    public bool IsCompletedStep => _current == SetupSteps.Completed;
    public bool IsSetupUnrealEngineStep => _current == SetupSteps.SetupUnreal;

    private Action<UserConfig?>? _onSaveAndContinue;
    public FirstTimeSetupViewModel()
        : this(
            Locator.Current.GetRequiredService<IPlasticService>(),
            Locator.Current.GetRequiredService<IGoogleAuthService>(),
            Locator.Current.GetRequiredService<IDependenciesService>(),
            Locator.Current.GetRequiredService<IConfigurationService>(),
            Locator.Current.GetRequiredService<IEditorService>()
            )
    {
    }
    public void OnSaveAndContinue(Action<UserConfig?> callback) => _onSaveAndContinue = callback;
    public FirstTimeSetupViewModel(IPlasticService plasticService, IGoogleAuthService googleAuthService, IDependenciesService dependenciesService, IConfigurationService configurationService, IEditorService editorService)
    {
        Workspace = new WorkspaceSetupViewModel(plasticService);
        Google = new GoogleSignInViewModel(googleAuthService);
        Dependencies = new DependenciesViewModel(dependenciesService);
        Setup = new SetupUnrealEngineViewModel(editorService);

        CreateWorkspace = ReactiveCommand.CreateFromTask(async () =>
        {
            if (await Workspace.CreateWorkspace())
            {
                ChangeCurrentStep(SetupSteps.SetupUnreal);
            }
        });
        SignInToGoogle = ReactiveCommand.CreateFromTask(async () =>
        {
            if (await Google.SignInToGoogle())
            {
                ChangeCurrentStep(SetupSteps.Dependencies);
            }
        });

        DownloadDependencies = ReactiveCommand.CreateFromTask(async () =>
        {
            if (await Dependencies.DownloadDependencies())
            {
                ChangeCurrentStep(SetupSteps.Completed);
            }
        });

        SetupUnrealEngine = ReactiveCommand.CreateFromTask(async () =>
        {
            if (await Setup.RunSetup(Workspace.Folder))
            {
                ChangeCurrentStep(SetupSteps.Google);
            }
        });

        SaveAndContinue = ReactiveCommand.CreateFromTask(async () =>
        {
            var userConfig = new UserConfig
            {
                ClientSecret = Google.Secret,
                LongtailPath = Dependencies.LongtailPath,
                Workspace = Workspace.Folder
            };
            await configurationService.SaveUserConfig(userConfig);
            _onSaveAndContinue?.Invoke(userConfig);
        });
    }

    private void ChangeCurrentStep(SetupSteps step)
    {
        _current = step;
        OnPropertyChanged(nameof(IsWorkspaceStep));
        OnPropertyChanged(nameof(IsGoogleStep));
        OnPropertyChanged(nameof(IsDependenciesStep));
        OnPropertyChanged(nameof(IsCompletedStep));
        OnPropertyChanged(nameof(IsSetupUnrealEngineStep));
    }
}