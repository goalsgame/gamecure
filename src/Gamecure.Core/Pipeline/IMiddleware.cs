namespace Gamecure.Core.Pipeline;

public record Context
{
    public Guid Id { get; } = Guid.NewGuid();
    public bool Failed { get; init; }
    public bool Succeeded => !Failed;
    public string? Reason { get; init; }
}
public delegate Task<TContext> ContextDelegate<TContext>(TContext context) where TContext : Context;

public interface IMiddleware<TContext> where TContext : Context
{
    bool ShouldRun(TContext context) => true;
    Task<TContext> OnInvoke(TContext context, ContextDelegate<TContext> next);
}