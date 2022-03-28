using System.Diagnostics;
using System.Globalization;

namespace Gamecure.Core.Common.Logging;

public class FileLogWriter : ILogWriter
{
    private readonly string _filename;
    private StreamWriter? _logstream;

    public FileLogWriter(string filename)
    {
        _filename = filename;
    }

    public async ValueTask OnStartup()
    {
        var folder = Path.GetDirectoryName(_filename);
        if (!Directory.Exists(folder) && folder != null)
        {
            Directory.CreateDirectory(folder);
        }
        var file = File.Open(_filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
        file.Seek(0, SeekOrigin.End);
        _logstream = new StreamWriter(file);
        _logstream.AutoFlush = true;
        await Write(LogLevel.Info, "Logger startup");
    }
    public ValueTask OnMessage(LogMessage logMessage) => Write(logMessage.Level, logMessage.Message);
    public ValueTask OnError(Exception exception) => Write(LogLevel.Error, $"Exception was thrown:  {exception.GetType().Name} - {exception.Message}");
    public async ValueTask OnDispose()
    {
        if (_logstream != null)
        {
            await Write(LogLevel.Info, "Disposing logger.");
            await _logstream.DisposeAsync();
            _logstream = null;
        }
    }

    private async ValueTask Write(LogLevel level, string message)
    {
        Debug.Assert(_logstream != null, "File has not been initialized.");
        await _logstream.WriteLineAsync($"{DateTimeNow()} | {level.ToString().ToUpper()} | {message}");
    }

    private static string DateTimeNow() => DateTime.Now.ToString("yyyy-MM-dd H:mm:ss zzz", new DateTimeFormatInfo());
}