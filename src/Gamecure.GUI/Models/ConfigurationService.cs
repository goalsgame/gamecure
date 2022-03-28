using System;
using System.IO;
using System.Threading.Tasks;
using Gamecure.Core;
using Gamecure.Core.Common;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Configuration;
using Gamecure.Core.Pipeline;

namespace Gamecure.GUI.Models;
public record UserConfig
{
    public string? Workspace { get; init; }
    public string? ClientSecret { get; init; }
    public string? LongtailPath { get; init; }
    public bool DeletePreviousEditor { get; init; } = true; // NOTE(Jens): The default behaviour is that we want to delete the old editor files before updating
}
internal class ConfigurationService : IConfigurationService
{
    private AppConfig? _appConfig;
    private UserConfig? _userConfig;

    private readonly string _userConfigPath = Path.Combine(GlobalConfiguration.CacheDirectory, "user.config");
    public async Task<AppConfig> GetAppConfig()
    {
        if (_appConfig != null)
        {
            return _appConfig;
        }
        var result = await new PipelineBuilder<AppConfigContext>()
            .With<ReadApplicationConfigFile>()
            .With<ValidateApplicationConfig>()
            .Build()
            .Invoke(new AppConfigContext("config.json"));

        if (result.Failed)
        {
            throw new Exception($"Failed to initialize the application: {result.Reason}");
        }
        _appConfig = result.Configuration;
        return _appConfig!;
    }

    public async Task<UserConfig?> GetUserConfig()
    {
        if (_userConfig != null)
        {
            return _userConfig;
        }

        Logger.Trace($"Loading user config from {_userConfigPath}");
        try
        {
            await using var file = File.Open(_userConfigPath, FileMode.Open);
            _userConfig = await Json.DeserializeAsync<UserConfig>(file);
            return _userConfig;
        }
        catch (FileNotFoundException)
        {
            Logger.Trace("Config file was not found, set default values.");
            return null;
        }
        catch (Exception e)
        {
            Logger.Error($"Opening the config file threw an exception. ({e.GetType()}) - {e.Message}");
            return null;
        }
    }

    public async Task SaveUserConfig(UserConfig config)
    {
        try
        {
            _userConfig = config;

            await using var file = File.Open(_userConfigPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            file.SetLength(0); // Reset the file
            await Json.SerializeAsync(file, _userConfig, true);
        }
        catch (Exception e)
        {
            Logger.Error("Failed to save the user config", e);
        }
    }
}