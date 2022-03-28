using Gamecure.Core.Common.Logging;

namespace Gamecure.Core.Pipeline;

public class PipelineBuilder<TContext> where TContext : Context
{
    private readonly HashSet<Type> _types = new();
    public PipelineBuilder<TContext> With<T>() where T : IMiddleware<TContext>
    {
        if (!_types.Add(typeof(T)))
        {
            throw new InvalidOperationException($"{typeof(T)} has already been added.");
        }
        return this;
    }

    public ContextDelegate<TContext> Build()
    {
        var middlewares = _types
            .Select(t => (IMiddleware<TContext>)(Activator.CreateInstance(t) ?? throw new InvalidOperationException($"Failed to create instance of {t.Name}")))
            .Reverse()
            .ToArray();
        ContextDelegate<TContext> current = Task.FromResult;
        foreach (var middleware in middlewares)
        {
            var next = current;
            current = async context =>
            {
                try
                {
                    if (middleware.ShouldRun(context))
                    {
                        var result = await middleware.OnInvoke(context, next);
                        if (result.Id != context.Id)
                        {
                            Logger.Warning("Mismatch in context ID. Keyword 'with' should be used instead of creating a new Context.");
                        }
                        return result;
                    }
                    return await next(context);

                }
                catch (Exception e)
                {
                    Logger.Error($"Exception in pipeline at middleware {middleware.GetType().Name}{Environment.NewLine}{e.GetType().Name}: {e.Message}");
                    throw;
                }
            };
        }
        return current;
    }
}