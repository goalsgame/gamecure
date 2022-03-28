using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Gamecure.Core;
using Gamecure.Core.Common.Logging;

namespace Gamecure.GUI.Models;

internal class LogFileService : ILogFileService
{
    public Task<LogEntry[]> GetAllLogFiles()
    {
        var files = Directory.EnumerateFiles(GlobalConfiguration.LogsDirectory, "*.log")
            .Select(filePath =>
            {
                var fileinfo = new FileInfo(filePath);
                var fileName = Path.GetFileName((string?)filePath) ?? string.Empty;
                return new LogEntry(fileinfo.CreationTime, fileName, filePath, fileinfo.Length);
            })
            .OrderByDescending(f => f.Created);
        return Task.FromResult(files.ToArray());
    }

    public async Task<LogLine[]> ReadLogFile(LogEntry entry)
    {
        var lines = await ReadLines(entry.FullPath);
        return lines.Select(CreateLogLine).ToArray();

        static LogLine CreateLogLine(string value)
        {
            var values = value.Split('|', 3, StringSplitOptions.TrimEntries);
            if (values.Length == 3)
            {
                return new LogLine(values[0], values[1], values[2]);
            }
            return new LogLine(string.Empty, string.Empty, value);
        }

        static async Task<string[]> ReadLines(string filename)
        {
            //NOTE(Jens): File.ReadAllLinesAsync does not work for the file that is currently being logged to. Not sure why since it should share the access.
            var file = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(file);
            List<string> lines = new();
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                lines.Add(line);
            }
            return lines.ToArray();
        }
    }

    public Task<bool> DeleteLogFile(LogEntry entry)
    {
        try
        {
            File.Delete(entry.FullPath);
            return Task.FromResult(true);
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to delete file {entry.FullPath}", e);
            return Task.FromResult(false);
        }
    }
}