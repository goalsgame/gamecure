using Gamecure.GUI.Models;
using Splat;

namespace Gamecure.GUI;

internal class Registry
{
    public static void Init(IMutableDependencyResolver services) =>
        services
            .RegisterLazySingleton<IConfigurationService>(_ => new ConfigurationService())
            .RegisterLazySingleton<IPlasticService>(resolver => new PlasticService(resolver.GetRequiredService<IConfigurationService>()))
            .RegisterLazySingleton<IGoogleAuthService>(resolver => new GoogleAuthService(resolver.GetRequiredService<IConfigurationService>()))
            .RegisterLazySingleton<IDependenciesService>(resolver => new DependenciesService(resolver.GetRequiredService<IConfigurationService>()))
            .RegisterLazySingleton<ILogFileService>(_ => new LogFileService())
            .RegisterLazySingleton<IEditorService>(resolver => new EditorService(resolver.GetRequiredService<IConfigurationService>(), resolver.GetRequiredService<IGoogleAuthService>()))
            .RegisterLazySingleton<IGamecureVersion>(resolver => new GamecureVersion(resolver.GetRequiredService<IConfigurationService>()))
        ;
}