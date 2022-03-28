using Gamecure.Core.Common;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Plastic;

public class CreateWorkspace : IMiddleware<PlasticContext>
{
    // Only create repo if the folder does not exist, or if there's no .plastic folder in the folder.
    public bool ShouldRun(PlasticContext context) => !Directory.Exists(context.Workspace) || !Directory.Exists(Path.Combine(context.Workspace, ".plastic"));

    public async Task<PlasticContext> OnInvoke(PlasticContext context, ContextDelegate<PlasticContext> next)
    {
        if (string.IsNullOrWhiteSpace(context.Config.Repository))
        {
            return context with { Failed = true, Reason = "Can't create a workspace when there's no repository specified. Make sure the repository is set in config.json" };
        }

        // Set the default name to Gamecure
        // NOTE(Jens): This will limit it to a single workspace created by Gamecure.
        const string workspaceName = "Gamecure";

        var path = context.Workspace;
        Directory.CreateDirectory(path);

        Logger.Info($"Creating workspace {workspaceName} in {path}");
        var result = await ProcessRunner.Run(context.PlasticCLIPath, $"workspace create {workspaceName} {path} {context.Config.Repository}", path);
        if (result.Success)
        {

            Logger.Info($"Workspace {workspaceName} successfully created");
            return await next(context with { RequiresConfiguration = true });
        }

        Logger.Error($"Failed to create the workspace");
        Logger.Error($"StdOut: {result.StdOutAsString()}");
        Logger.Error($"StdErr: {result.StdErrAsString()}");
        return context with { Failed = true, Reason = $"Failed to create the workspace. Error: {result.StdErrAsString()}" };
    }
}
