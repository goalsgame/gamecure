using System.Threading.Channels;

namespace Gamecure.Core.Common.Logging;

public enum LogLevel
{
    Error,
    Info,
    Debug,
    Warning,
    Trace
}

public struct LogMessage
{
    public readonly string Message;
    public readonly LogLevel Level;

    internal LogMessage(LogLevel level, string message)
    {
        Level = level;
        Message = message;
    }
}

public static class Logger
{
    private static readonly Channel<LogMessage> _channel = Channel.CreateUnbounded<LogMessage>();

    public static void Flush()
    {
        _channel.Writer.Complete();
    }

    public static void Trace(string message) => Log(LogLevel.Trace, message);
    public static void Error(string message) => Log(LogLevel.Error, message);
    public static void Error(string message, Exception e) =>
        Log(LogLevel.Error, $"{message} - {e.GetType().Name} {e.Message}{Environment.NewLine}{e.StackTrace}");
    public static void Info(string message) => Log(LogLevel.Info, message);
    public static void Warning(string message) => Log(LogLevel.Warning, message);
    private static void Log(LogLevel level, string message) =>
        _channel.Writer.TryWrite(new LogMessage(level, message));

    public static IDisposable Start() => new LoggerLifetime(new ConsoleLogWriter());
    public static IDisposable Start(ILogWriter logWriter) => new LoggerLifetime(logWriter);
    private class LoggerLifetime : IDisposable
    {
        private readonly Task _logTask;
        private static bool _active = true;
        private readonly ILogWriter _writer;

        public LoggerLifetime(ILogWriter logWriter)
        {
            _writer = logWriter;
            _logTask = Task.Run(RunBackgroundThread);
        }

        private async Task RunBackgroundThread()
        {
            var reader = _channel.Reader;
            try
            {
                await _writer.OnStartup();
            }
            catch (Exception e)
            {
                await OnError(e);
            }

            while (_active)
            {
                if (reader.TryRead(out var logMessage))
                {
                    try
                    {
                        await _writer.OnMessage(logMessage);
                    }
                    catch (Exception e)
                    {
                        await OnError(e);
                    }
                }
                else
                {
                    await Task.Delay(10);
                }
            }

            // Flush the remaining messages
            while (reader.TryRead(out var logMessage))
            {
                await _writer.OnMessage(logMessage);
            }
            await _writer.OnDispose();
        }

        private async Task OnError(Exception exception)
        {
            try
            {
                await _writer.OnError(exception);
            }
            catch (Exception e)
            {
                await Console.Error.WriteLineAsync($"Exception was thrown when {nameof(OnError)} was called. {e.GetType().Name} - {e.Message}");
            }
        }

        public void Dispose()
        {
            _active = false;
            Flush();
            _logTask.Wait();
        }
    }
}
