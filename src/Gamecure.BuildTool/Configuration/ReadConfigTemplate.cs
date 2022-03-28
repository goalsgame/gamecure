using Gamecure.Core.Common;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Configuration;
using Gamecure.Core.Pipeline;

namespace Gamecure.BuildTool.Configuration;

internal class ReadConfigTemplate : IMiddleware<ConfigContext>
{
    public bool ShouldRun(ConfigContext context) => context.Template != null;
    public async Task<ConfigContext> OnInvoke(ConfigContext context, ContextDelegate<ConfigContext> next)
    {
        var templatePath = Path.IsPathFullyQualified(context.Template!)
            ? context.Template
            : Path.Combine(Directory.GetCurrentDirectory(), context.Template!);

        if (templatePath == null)
        {
            return context with { Failed = true, Reason = "failed to get the template path" };
        }

        if (!File.Exists(templatePath))
        {
            return context with { Failed = true, Reason = $"Template file was not found at path: {templatePath}" };
        }

        Logger.Trace($"Loading template from: {templatePath}");
        var config = await LoadTemplate(templatePath);
        return await next(context with
        {
            AppConfig = config
        });


        static async Task<AppConfig?> LoadTemplate(string filename)
        {
            await using var file = File.OpenRead(filename);
            return await Json.DeserializeAsync<AppConfig>(file);
        }
    }
}