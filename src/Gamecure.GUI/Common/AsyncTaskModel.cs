using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Gamecure.GUI.Common;

public class AsyncTaskModel<TResult> : IAsyncTask<TResult>, INotifyPropertyChanged where TResult : class?
{
    private readonly Func<Task<AsyncTaskResult<TResult?>>> _task;
    private TResult? _result;

    public bool IsWaiting => _state is AsyncTaskState.Waiting;
    public bool IsRunning => _state is AsyncTaskState.Running;
    public bool IsCompleted => _state is AsyncTaskState.Completed;
    public bool Failed => _state is AsyncTaskState.Error;
    public bool RunOnReset { get; set; }
    public object? Result => _result;

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    private AsyncTaskState _state = AsyncTaskState.Waiting;

    public AsyncTaskModel(Task task)
    {
        _task = async () =>
        {
            await task;
            return AsyncTaskResult<TResult?>.Success(null);
        };
        var _ = Execute(false);
    }

    public AsyncTaskModel(Func<Task> task, bool startImmediately = false)
    {
        _task = async () =>
        {
            await task();
            return AsyncTaskResult<TResult?>.Success(null);
        };
        if (startImmediately)
        {
            var _ = Execute(false);
        }
    }

    public AsyncTaskModel(Func<Task<TResult?>> task, bool startImmediately = false)
    {
        _task = async () => AsyncTaskResult<TResult?>.Success(await task());
        if (startImmediately)
        {
            var _ = Execute(false);
        }
    }

    public AsyncTaskModel(Func<Task<AsyncTaskResult<TResult?>>> task, bool startImmediately = false)
    {
        _task = task;
        if (startImmediately)
        {
            var _ = Execute(false);
        }
    }

    public async Task<TResult?> Execute(bool reset)
    {
        if (reset)
        {
            ResetInternal();
        }
        if (_state == AsyncTaskState.Waiting)
        {
            UpdateState(AsyncTaskState.Running);
            await ExecuteInternal();
            return _result;
        }

        if (_state == AsyncTaskState.Completed)
        {
            return _result;
        }
        throw new InvalidOperationException($"Can't execute a task that is in state: {_state}");
    }

    private async Task ExecuteInternal()
    {
        try
        {
            var task = _task();
            var asyncTaskResult = await task;
            if (asyncTaskResult.Error)
            {
                ErrorMessage = asyncTaskResult.Reason;
                UpdateState(AsyncTaskState.Error);
            }
            else
            {
                _result = asyncTaskResult.Result;
                UpdateState(AsyncTaskState.Completed);
            }
        }
        catch (Exception e)
        {
            ErrorMessage = $"{e.GetType().Name}: {e.Message}";
            UpdateState(AsyncTaskState.Error);
        }
    }

    private void UpdateState(AsyncTaskState state)
    {
        _state = state;
        OnPropertyChanged(nameof(IsWaiting));
        OnPropertyChanged(nameof(IsRunning));
        OnPropertyChanged(nameof(IsCompleted));
        OnPropertyChanged(nameof(Failed));
        OnPropertyChanged(nameof(Result));
    }

    public void Reset()
    {
        ResetInternal();
        if (RunOnReset)
        {
            var _ = Execute(false);
        }
    }

    private void ResetInternal()
    {
        ErrorMessage = null;
        UpdateState(AsyncTaskState.Waiting);
    }


    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}