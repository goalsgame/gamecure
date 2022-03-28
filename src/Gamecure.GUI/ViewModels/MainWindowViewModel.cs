using Avalonia.Controls;
using Gamecure.GUI.Common;
using Gamecure.GUI.Models;
using Gamecure.GUI.Views;
using Splat;

namespace Gamecure.GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public IAsyncTask<string> LoadConfigTask { get; }


    private UserControl? _content;
    public UserControl? Content
    {
        get => _content;
        private set => SetProperty(ref _content, value);
    }

    public MainWindowViewModel()
        : this(Locator.Current.GetRequiredService<IConfigurationService>())
    {
    }

    public MainWindowViewModel(IConfigurationService configurationService)
    {
        LoadConfigTask = new AsyncTaskModel<string>(async () =>
        {
            var config = await configurationService.GetUserConfig();
            if (config == null)
            {
                var setup = new FirstRunSetup();
                setup.ViewModel?.OnSaveAndContinue(_ =>
                {
                    Content = new MainView();
                });
                Content = setup;
            }
            else
            {
                Content = new MainView();
            }
            return string.Empty;
        });
        var _ = LoadConfigTask.Execute();
    }
}