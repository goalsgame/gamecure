using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Plastic;

public class ValidatePlasticWorkspace : IMiddleware<PlasticContext>
{
    public async Task<PlasticContext> OnInvoke(PlasticContext context, ContextDelegate<PlasticContext> next)
    {
        var workspace = context.Workspace;
        if (string.IsNullOrWhiteSpace(workspace))
        {
            return context with { Failed = true, Reason = "Workspace has not been set." };
        }

        var plasticDirectory = Path.Combine(workspace, ".plastic");
        if (!Directory.Exists(plasticDirectory))
        {
            return context with { Failed = true, Reason = $"{workspace} is not a plastic workspace. A .plastic directory could not be found." };
        }

        var engineDirectory = Path.Combine(workspace, "Engine");
        if (!Directory.Exists(engineDirectory))
        {
            return context with { Failed = true, Reason = $"The path does not contain an Engine directory. {engineDirectory} could not be found." };
        }

        var gameDirectory = Path.Combine(workspace, "Game");
        if (!Directory.Exists(gameDirectory))
        {
            return context with { Failed = true, Reason = $"The path does not contain a Game directory. {gameDirectory} could not be found." };
        }

        Logger.Trace($"Set plastic workspace to: {workspace}");
        return await next(context);
    }
}