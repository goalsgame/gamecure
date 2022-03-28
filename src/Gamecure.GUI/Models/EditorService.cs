using System;
using System.Threading.Tasks;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Configuration;
using Gamecure.Core.Editor;
using Gamecure.Core.Editor.DownloadAndCopy;
using Gamecure.Core.Editor.Setup;
using Gamecure.Core.Editor.Versions;
using Gamecure.Core.Pipeline;

namespace Gamecure.GUI.Models;

public record EditorVersionResult(int? CurrentVersion, EditorVersion[] Versions);
internal class EditorService : IEditorService
{
    private readonly IConfigurationService _configurationService;
    private readonly IGoogleAuthService _googleAuthService;

    public EditorService(IConfigurationService configurationService, IGoogleAuthService googleAuthService)
    {
        _configurationService = configurationService;
        _googleAuthService = googleAuthService;
    }

    public async Task<EditorVersionResult> GetVersions()
    {
        var userConfig = await _configurationService.GetUserConfig();
        if (userConfig == null)
        {
            throw new InvalidOperationException($"{nameof(UserConfig)} was null.");
        }

        var appConfig = await _configurationService.GetAppConfig();
        if (appConfig == null)
        {
            throw new InvalidOperationException($"{nameof(AppConfig)} was null.");
        }

        var googleAuth = await _googleAuthService.Run(userConfig.ClientSecret!);
        if (googleAuth.Failed)
        {
            throw new InvalidOperationException($"Google Auth failed with reason: {googleAuth.Reason}");
        }

        var result = await new PipelineBuilder<EditorVersionsContext>()
            .With<ReadCurrentVersion>()
            .With<SetupEditorVersionUrls>()
            .With<GetEditorVersions>()
            .Build()
            .Invoke(new EditorVersionsContext(appConfig.Container, googleAuth.AccessToken));

        if (result.Failed)
        {
            Logger.Error($"GetVersions failed with reason: {result.Reason}");
        }
        return new EditorVersionResult(result.CurrentEditorVersion, result.Versions);
    }

    public async Task<DownloadEditorContext> DownloadEditor(EditorVersion version)
    {
        var userConfig = await _configurationService.GetUserConfig();
        if (userConfig == null)
        {
            throw new InvalidOperationException($"{nameof(UserConfig)} was null.");
        }

        var appConfig = await _configurationService.GetAppConfig();
        if (appConfig == null)
        {
            throw new InvalidOperationException($"{nameof(AppConfig)} was null.");
        }

        var googleAuth = await _googleAuthService.Run(userConfig.ClientSecret!);
        if (googleAuth.Failed)
        {
            throw new InvalidOperationException($"Google Auth failed with reason: {googleAuth.Reason}");
        }

        var result = await new PipelineBuilder<DownloadEditorContext>()
            .With<CheckIfEditorIsRunning>()
            .With<StoreEditorVersion>()
            .With<WriteTemporaryGoogleCredentials>()
            .With<DownloadEditor>()
            .With<ValidateAndSetEditorPath>()
            .With<TrackCopiedFiles>()
            .With<DeletePreviousEditor>()
            .With<CopyEditor>()
            .Build()
            .Invoke(new DownloadEditorContext(userConfig.Workspace!, userConfig.LongtailPath!, appConfig.Container, googleAuth.RefreshToken, appConfig.ClientId, googleAuth.ClientSecret, version)
            {
                DeleteCopiedFiles = userConfig.DeletePreviousEditor
            });

        if (result.Failed)
        {
            Logger.Error($"Download editor failed with reason: {result.Reason}");
        }
        return result;
    }

    public async Task<EditorSetupContext> RunSetup(string? workspace)
    {
        if (workspace == null)
        {
            var userConfig = await _configurationService.GetUserConfig();
            workspace = userConfig?.Workspace ?? string.Empty;
        }

        var result = await new PipelineBuilder<EditorSetupContext>()
            .With<RunSetupScript>()
            .Build()
            .Invoke(new EditorSetupContext(workspace));

        if (result.Failed)
        {
            Logger.Error($"Failed to run setup with reason: {result.Reason}");
        }
        return result;
    }
}