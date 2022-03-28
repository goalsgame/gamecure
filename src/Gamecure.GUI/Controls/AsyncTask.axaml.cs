using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Gamecure.GUI.Common;

namespace Gamecure.GUI.Controls;

public partial class AsyncTask : UserControl
{
    public static readonly StyledProperty<IAsyncTask> TaskProperty = AvaloniaProperty.Register<AsyncTask, IAsyncTask>(nameof(Task));
    public IAsyncTask Task
    {
        get => GetValue(TaskProperty);
        set => SetValue(TaskProperty, value);
    }

    public static readonly StyledProperty<string> MessageProperty = AvaloniaProperty.Register<AsyncTask, string>(nameof(Message));
    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly StyledProperty<bool> ShowContentProperty = AvaloniaProperty.Register<AsyncTask, bool>(nameof(ShowContent));
    public bool ShowContent
    {
        get => GetValue(ShowContentProperty);
        set => SetValue(ShowContentProperty, value);
    }

    public static readonly DirectProperty<AsyncTask, bool> RunOnRetryProperty = AvaloniaProperty.RegisterDirect<AsyncTask, bool>(nameof(RunOnRetry), asyncTask => asyncTask.Task.RunOnReset, (asyncTask, runOnRetry) => asyncTask.Task.RunOnReset = runOnRetry);
    public bool RunOnRetry
    {
        get => GetValue(RunOnRetryProperty);
        set => SetValue(RunOnRetryProperty, value);
    }

    public AsyncTask()
    {
        InitializeComponent();
        Message = string.Empty;
        Task = new EmptyTask();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private class EmptyTask : IAsyncTask
    {
        public bool IsWaiting => true;
        public bool IsRunning => false;
        public bool IsCompleted => false;
        public bool Failed => false;
        public object? Result => null;
        public Task<T?> Execute<T>() where T : class => System.Threading.Tasks.Task.FromResult((T?)null);
        public Task Execute(bool reset = false) => System.Threading.Tasks.Task.CompletedTask;
        public void Reset() { }

        public bool RunOnReset { get; set; } = false;
    }
}