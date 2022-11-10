using System.Threading.Tasks;
using Gamecure.Core.Editor.Setup;
using Gamecure.GUI.Common;
using Gamecure.GUI.Models;

namespace Gamecure.GUI.ViewModels.FirstTimeSetup;

public class SetupUnrealEngineViewModel : ViewModelBase
{
    public IAsyncTask<EditorSetupContext> SetupUnrealEngineTask { get; }

    private string? _workspace;

    public SetupUnrealEngineViewModel(IEditorService editorService)
    {
        SetupUnrealEngineTask = new AsyncTaskModel<EditorSetupContext>(async () =>
        {
            var result = await editorService.RunSetup(_workspace);
            return result.Failed
                ? AsyncTaskResult<EditorSetupContext?>.Failed(result.Reason)
                : AsyncTaskResult<EditorSetupContext?>.Success(result);
        });
    }

    public async Task<bool> RunSetup(string? workspace = null)
    {
        _workspace = workspace;
        var result = await SetupUnrealEngineTask.Execute(true);
        return result?.Succeeded ?? false;
    }
}