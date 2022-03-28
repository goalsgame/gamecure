namespace Gamecure.Core.Common.Logging;

public interface ILogWriter
{
    ValueTask OnStartup() => ValueTask.CompletedTask;
    ValueTask OnMessage(LogMessage logMessage);
    ValueTask OnDispose() => ValueTask.CompletedTask;
    ValueTask OnError(Exception exception);
}