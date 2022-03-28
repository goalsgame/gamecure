using System;
using System.Threading.Tasks;

namespace Gamecure.GUI.Models;

public record LogEntry(DateTime Created, string Filename, string FullPath, long Size);
public record struct LogLine(string Date, string Level, string Value);

internal interface ILogFileService
{
    Task<LogEntry[]> GetAllLogFiles();
    Task<LogLine[]> ReadLogFile(LogEntry entry);
    Task<bool> DeleteLogFile(LogEntry entry);
}