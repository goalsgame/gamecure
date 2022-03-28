using Gamecure.Core.Common;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Editor.Setup;

public class RunSetupScript : IMiddleware<EditorSetupContext>
{
    public async Task<EditorSetupContext> OnInvoke(EditorSetupContext context, ContextDelegate<EditorSetupContext> next)
    {
        var setupPath = GlobalConfiguration.Platform switch
        {
            Platform.Macos or Platform.Linux => Path.Combine(context.Workspace, "Setup.sh"),
            Platform.Windows => Path.Combine(context.Workspace, "Setup.bat"),
            _ => throw new NotSupportedException("Platform not supported.")
        };

        Logger.Info($"Running Unreal Engine Setup script from path {setupPath}");
        if (!File.Exists(setupPath))
        {
            return context with { Failed = true, Reason = $"The setup script could not be found at path {setupPath}" };
        }

        var result = await ProcessRunner.Run(setupPath, "--force", context.Workspace, TimeSpan.FromMinutes(45), createNewWindow: true, redirectOutput: false);
        if (result.Success)
        {
            return await next(context);
        }

        Logger.Error("Unreal Engine Setup script failed.");
        Logger.Error($"StdErr: {result.StdErrAsString()}");
        Logger.Error($"StdOut: {result.StdOutAsString()}");

        return context with { Failed = true, Reason = "The setup script failed, see the log for more information." };
    }
}