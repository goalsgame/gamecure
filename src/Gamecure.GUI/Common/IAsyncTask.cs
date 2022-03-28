using System.Threading.Tasks;

namespace Gamecure.GUI.Common;


public interface IAsyncTask<TResult> : IAsyncTask where TResult : class?
{
    Task<TResult?> Execute(bool reset = false);
}

public interface IAsyncTask
{
    bool IsWaiting { get; }
    bool IsRunning { get; }
    bool IsCompleted { get; }
    bool Failed { get; }
    object? Result { get; }
    void Reset();
    bool RunOnReset { get; set; }
}