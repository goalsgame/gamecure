using System.Threading.Tasks;
using Gamecure.Core.Longtail;
using Gamecure.GUI.Common;
using Gamecure.GUI.Models;

namespace Gamecure.GUI.ViewModels.FirstTimeSetup;

public class DependenciesViewModel : ViewModelBase
{
    public IAsyncTask<LongtailContext> DependenciesTask { get; }

    private string? _longtailPath;
    public string? LongtailPath
    {
        get => _longtailPath;
        set => SetProperty(ref _longtailPath, value);
    }

    private bool _reset;
    public bool Reset
    {
        get => _reset;
        set => SetProperty(ref _reset, value);
    }

    public DependenciesViewModel(IDependenciesService dependenciesService)
    {
        DependenciesTask = new AsyncTaskModel<LongtailContext>(async () =>
        {
            var result = await dependenciesService.Run(Reset);
            return result.Failed
                ? AsyncTaskResult<LongtailContext?>.Failed(result.Reason)
                : AsyncTaskResult<LongtailContext?>.Success(result);
        });
    }

    public async Task<bool> DownloadDependencies()
    {
        var result = await DependenciesTask.Execute(true);
        Reset = false;
        if (result?.Succeeded ?? false)
        {
            LongtailPath = result.LongtailPath;
            return true;
        }
        return false;
    }
}