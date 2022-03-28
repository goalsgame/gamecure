using Gamecure.Core.Common;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.BuildTool.Configuration;

internal class WriteConfig : IMiddleware<ConfigContext>
{
    public async Task<ConfigContext> OnInvoke(ConfigContext context, ContextDelegate<ConfigContext> next)
    {
        var filename = string.IsNullOrWhiteSpace(context.OutputFilename) ? "config.json" : context.OutputFilename;
        var configPath = Path.IsPathFullyQualified(filename)
            ? context.OutputFilename
            : Path.Combine(Directory.GetCurrentDirectory(), filename);

        if (configPath == null)
        {
            return context with { Failed = true, Reason = "Failed to set the config path." };
        }
        if (!context.Overwrite && File.Exists(configPath))
        {
            return context with { Failed = true, Reason = $"File {configPath} already exist, use --force to overwrite the file." };
        }

        Logger.Trace($"Writing config file to {configPath}. Overwrite: {context.Overwrite}");
        await using var configFile = File.Open(configPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        configFile.SetLength(0);
        await Json.SerializeAsync(configFile, context.AppConfig, true);

        return await next(context);
    }
}