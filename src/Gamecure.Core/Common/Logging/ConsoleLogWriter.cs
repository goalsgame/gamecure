using System.Globalization;

namespace Gamecure.Core.Common.Logging;

public class ConsoleLogWriter : ILogWriter
{
    public ValueTask OnMessage(LogMessage logMessage)
    {
        static string DateTimeNow() => DateTime.Now.ToString("HH:mm:ss.fff", new DateTimeFormatInfo());
        var color = logMessage.Level switch
        {
            LogLevel.Debug or LogLevel.Trace => ConsoleColor.Cyan,
            LogLevel.Info => ConsoleColor.DarkCyan,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            _ => throw new ArgumentOutOfRangeException()
        };
        Console.ResetColor();
        Console.Write("{0} [", DateTimeNow());
        Console.ForegroundColor = color;
        Console.Write(logMessage.Level);
        Console.ResetColor();
        Console.WriteLine("] - {0}", logMessage.Message);
        return ValueTask.CompletedTask;
    }

    public ValueTask OnError(Exception exception)
    {
        Console.ResetColor();
        Console.Error.WriteLine($"Exception was thrown when writing the log: {exception.Message}");
        return ValueTask.CompletedTask;
    }
}