using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Gamecure.Core.Plastic;
using Gamecure.GUI.Common;
using Gamecure.GUI.Models;
using ReactiveUI;

namespace Gamecure.GUI.ViewModels.FirstTimeSetup;

public class WorkspaceSetupViewModel : ViewModelBase
{
    public IAsyncTask<PlasticContext> WorkspaceTask { get; }
    public ICommand Browse { get; }

    private string? _folder;
    public string? Folder
    {
        get => _folder;
        set => SetProperty(ref _folder, value);
    }

    public WorkspaceSetupViewModel(IPlasticService plasticService)
    {
        WorkspaceTask = new AsyncTaskModel<PlasticContext>(async () =>
        {
            var result = await plasticService.Run(Folder);
            return result.Failed
                ? AsyncTaskResult<PlasticContext?>.Failed(result.Reason)
                : AsyncTaskResult<PlasticContext?>.Success(result);
        });

        Browse = ReactiveCommand.CreateFromTask(async () =>
        {
            var mainWindow = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : throw new InvalidOperationException("Failed to find the mainwindow");
            var dialog = new OpenFolderDialog
            {
                Directory = Folder
            };
            var selectedFolder = await dialog.ShowAsync(mainWindow);
            if (selectedFolder != null)
            {
                Folder = selectedFolder;
            }
        });
    }

    public async Task<bool> CreateWorkspace()
    {
        var result = await WorkspaceTask.Execute(true);

        return result?.Succeeded ?? false;
    }
}