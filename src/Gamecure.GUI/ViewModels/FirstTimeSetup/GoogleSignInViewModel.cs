using System.Threading.Tasks;
using Gamecure.Core.Google;
using Gamecure.GUI.Common;
using Gamecure.GUI.Models;

namespace Gamecure.GUI.ViewModels.FirstTimeSetup;

public class GoogleSignInViewModel : ViewModelBase
{
    public IAsyncTask<GoogleAuthContext> SignInTask { get; }

    private string? _secret;
    public string? Secret
    {
        get => _secret;
        set => SetProperty(ref _secret, value);
    }
    private bool _reset;
    public bool Reset
    {
        get => _reset;
        set => SetProperty(ref _reset, value);
    }

    public string? RefreshToken { get; private set; }

    public GoogleSignInViewModel(IGoogleAuthService googleAuthService)
    {
        SignInTask = new AsyncTaskModel<GoogleAuthContext>(async () =>
        {
            var result = await googleAuthService.Run(Secret!, Reset);
            return result.Failed
                ? AsyncTaskResult<GoogleAuthContext?>.Failed(result.Reason)
                : AsyncTaskResult<GoogleAuthContext?>.Success(result);
        });
    }

    public async Task<bool> SignInToGoogle()
    {
        var result = await SignInTask.Execute(true);
        RefreshToken = result?.RefreshToken;
        Reset = false;
        return result?.Succeeded ?? false;
    }
}