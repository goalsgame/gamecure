using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DynamicData;
using Gamecure.GUI.Common;
using Gamecure.GUI.Models;
using ReactiveUI;
using Splat;

namespace Gamecure.GUI.ViewModels;
internal class AppLogViewModel : ViewModelBase
{
    public IAsyncTask<LogEntry[]> GetLogFilesTask { get; }
    public IAsyncTask<LogLine[]> LogEntryTask { get; }
    public ObservableCollection<LogEntry> LogEntries { get; } = new();

    public LogEntry? _selectedItem;
    public LogEntry? SelectedItem
    {
        get => _selectedItem;
        set
        {
            SetProperty(ref _selectedItem, value);
            var _ = LogEntryTask.Execute(true);
        }
    }

    private LogLine[] _selectedLog = Array.Empty<LogLine>();
    public LogLine[] SelectedLog
    {
        get => _selectedLog;
        set => SetProperty(ref _selectedLog, value);
    }

    public ICommand DeleteLogFile { get; }

    public AppLogViewModel()
        : this(Locator.Current.GetRequiredService<ILogFileService>())
    {
    }

    public AppLogViewModel(ILogFileService logFileService)
    {
        DeleteLogFile = ReactiveCommand.CreateFromTask<LogEntry?>(async entry =>
        {
            if (entry != null && await logFileService.DeleteLogFile(entry))
            {
                LogEntries.Remove(entry);
            }
        });

        GetLogFilesTask = new AsyncTaskModel<LogEntry[]>(async () =>
        {
            var files = await logFileService.GetAllLogFiles();
            LogEntries.Clear();
            LogEntries.AddRange(files);
            return files;
        });

        LogEntryTask = new AsyncTaskModel<LogLine[]>(async () =>
        {
            var logEntry = SelectedItem;
            if (logEntry != null)
            {
                SelectedLog = await logFileService.ReadLogFile(logEntry);
            }
            return Array.Empty<LogLine>();
        });

        var _ = GetLogFilesTask.Execute();
    }
}
