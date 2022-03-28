using System.Net.NetworkInformation;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Google;

public class GoogleCallbackUrl : IMiddleware<GoogleAuthContext>
{
    private const int _portMin = 8400;
    private const int _portMax = 9600;
    private readonly Random _random = new();
    public async Task<GoogleAuthContext> OnInvoke(GoogleAuthContext context, ContextDelegate<GoogleAuthContext> next)
    {
        // NOTE(Jens): This might crash at some point because it can signal that the same port is available for 2 pipelines. We can fix that by keeping a lock and storing the ports that are being used, and remove them at the end of the pipeline.
        var tries = 10;
        int port;
        do
        {
            port = _random.Next(_portMin, _portMax);
        } while (!IsAvailable(port) || tries-- <= 0);

        if (tries <= 0)
        {
            return context with { Failed = true, Reason = "Failed to find an available port. Please try again." };
        }

        return await next(context with
        {
            CallbackUrl = $"http://127.0.0.1:{port}/"
        });

        static bool IsAvailable(int port) => IPGlobalProperties
            .GetIPGlobalProperties()
            .GetActiveTcpConnections()
            .All(tcpi => tcpi.LocalEndPoint.Port != port);
    }
}