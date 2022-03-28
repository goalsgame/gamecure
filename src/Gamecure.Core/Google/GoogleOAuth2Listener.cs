using System.Diagnostics;
using System.Net;
using System.Text;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Google;

public class GoogleOAuth2Listener : IMiddleware<GoogleAuthContext>
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(1);
    public async Task<GoogleAuthContext> OnInvoke(GoogleAuthContext context, ContextDelegate<GoogleAuthContext> next)
    {
        if (context.CallbackUrl == string.Empty)
        {
            return context with { Failed = true, Reason = "CallbackUrl has not been set, can't create a HttpListener" };
        }

        Logger.Trace($"Creating a HttpListener with callback url {context.CallbackUrl}");
        var listener = new HttpListener();
        listener.Prefixes.Add(context.CallbackUrl);
        listener.Start();
        var listenerTask = Task.Run(async () =>
        {
            try
            {
                while (true)
                {
                    var requestContext = await listener.GetContextAsync();
                    var code = requestContext.Request.QueryString.Get("code");
                    if (!string.IsNullOrWhiteSpace(code))
                    {
                        requestContext.Response.StatusCode = 200;
                        requestContext.Response.Close(Encoding.UTF8.GetBytes("Success!"), true);
                        return code;
                    }
                    Logger.Warning("Recieved a call to the callback without a code.");
                    Logger.Warning($"RawUrl: {requestContext.Request.RawUrl}");
                    requestContext.Response.StatusCode = 400;
                    requestContext.Response.Close(Encoding.UTF8.GetBytes("Code missing.."), true);
                }
            }
            catch
            {
                return null;
            }
        });

        try
        {
            context = await next(context);
            var code = await WaitForAuthCode();
            if (code == null)
            {
                return context with { Failed = true, Reason = "Failed to authenticate with google." };
            }
            return context with { AuthCode = code };
        }
        finally
        {
            listener.Stop();
        }

        async Task<string?> WaitForAuthCode()
        {
            var timer = Stopwatch.StartNew();
            while (timer.Elapsed < Timeout)
            {
                if (listenerTask.IsCompleted)
                {
                    return await listenerTask;
                }
                await Task.Delay(100);
            }
            Logger.Warning($"Timeout reached while waiting for the auth code: {Timeout}");
            return null;
        }
    }
}