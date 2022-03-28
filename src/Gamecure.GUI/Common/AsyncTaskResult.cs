namespace Gamecure.GUI.Common;

public class AsyncTaskResult<TResult> where TResult : class?
{
    public TResult? Result { get; set; }
    public bool Error { get; set; }
    public string? Reason { get; set; }

    public static AsyncTaskResult<TResult> Success(TResult result) => new(result);
    public static AsyncTaskResult<TResult> Failed(string? reason = null) => new(reason);
    private AsyncTaskResult(TResult result) => Result = result;

    private AsyncTaskResult(string? reason)
    {
        Error = true;
        Reason = reason;
    }
}