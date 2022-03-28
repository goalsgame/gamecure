using System;
using Splat;

namespace Gamecure.GUI;

internal static class DependencyInjectionExtensions
{
    public static T GetRequiredService<T>(this IReadonlyDependencyResolver resolver)
    {
        if (resolver is null)
        {
            throw new ArgumentNullException(nameof(resolver));
        }

        var service = resolver.GetService(typeof(T));
        if (service == null)
        {
            throw new InvalidOperationException($"Service {typeof(T).Name} has not been registered.");
        }
        return (T)service;
    }

    public static IMutableDependencyResolver RegisterLazySingleton<T>(this IMutableDependencyResolver services, Func<IReadonlyDependencyResolver, object> factory)
    {
        services.RegisterLazySingleton(() => factory((IReadonlyDependencyResolver)services), typeof(T));
        return services;
    }
}