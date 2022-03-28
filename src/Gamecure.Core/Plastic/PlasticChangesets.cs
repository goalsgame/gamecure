using Gamecure.Core.Common;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;
using Gamecure.Core.Plastic.Parser;

namespace Gamecure.Core.Plastic;

public class PlasticChangesets : IMiddleware<PlasticContext>
{
    public async Task<PlasticContext> OnInvoke(PlasticContext context, ContextDelegate<PlasticContext> next)
    {
        //NOTE(Jens) Start both tasks (plastic is slow, don't want to run them after eachother)
        var changesetsTask = ProcessRunner.Run(context.PlasticCLIPath, $"log --csformat=\"{LogParser.LogFormat}\" --itemformat=\"{LogParser.ItemsFormat}\"", context.Workspace, timeout: TimeSpan.FromMinutes(2));

        // Plastic changeset tracking is disabled for now, it's not reliable
        //var statusResult = await ProcessRunner.Run(context.PlasticCLIPath, $"status --header", context.Workspace, timeout: TimeSpan.FromMinutes(2));
        //if (!statusResult.Success)
        //{
        //    Logger.Error($"cm status failed with code {statusResult.ExitCode}");
        //    Logger.Error($"StdErr: {statusResult.StdErrAsString()}");
        //    Logger.Error($"StdOut: {statusResult.StdOutAsString()}");
        //    return context with { Failed = true, Reason = $"cm status failed with code {statusResult.ExitCode}" };
        //}

        var changesetsResult = await changesetsTask;
        if (!changesetsResult.Success)
        {
            Logger.Error($"cm log failed with code {changesetsResult.ExitCode}");
            Logger.Error($"StdErr: {changesetsResult.StdErrAsString()}");
            Logger.Error($"StdOut: {changesetsResult.StdOutAsString()}");
            return context with { Failed = true, Reason = $"cm log command failed with code {changesetsResult.ExitCode} and std err: {changesetsResult.StdErrAsString()}" };
        }

        try
        {
            var changesets = LogParser
                .Parse(changesetsResult.StdOut)
                .ToArray();

            if (changesets.Length == 0)
            {
                return context with { Failed = true, Reason = "No changesets were found, are you in the correct workspace?" };
            }

            // Plastic changeset tracking is disabled for now, it's not reliable
            //var status = GetPlasticStatus(statusResult);
            //if (status != null)
            //{
            //    Logger.Trace($"Current workspace. Head: {status.Head} Branch: {status.Branch} Repository: {status.Repository} Server: {status.Server} Type: {status.ServerType}");
            //}

            return await next(context with
            {
                //Status = status,
                Changesets = changesets
            });
        }
        catch (Exception e)
        {
            Logger.Error("Failed to parse log or status", e);
            return context with { Failed = true, Reason = $"Failed to parse log or status with message: {e.Message}" };
        }

        //static PlasticStatus? GetPlasticStatus(ProcessResult result)
        //{
        //    try
        //    {
        //        return StatusParser.Parse(result.StdOut.FirstOrDefault());
        //    }
        //    catch (Exception e)
        //    {
        //        Logger.Error("Failed to parse plastic status", e);
        //    }
        //    return null;
        //}
    }
}