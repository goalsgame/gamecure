using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.ReactiveUI;
using Gamecure.Core;
using Gamecure.Core.Common.Logging;
using Splat;

namespace Gamecure.GUI;

internal class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        GlobalConfiguration.Init();
        using var _ = Logger.Start(new FileLogWriter(GlobalConfiguration.LogFilePath));

        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
        TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        Registry.Init(Locator.CurrentMutable);
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
    }

    private static void TaskSchedulerOnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        try
        {
            foreach (var exception in e.Exception.InnerExceptions)
            {
                Logger.Error("UnobservedTaskException", exception);
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            if (e.ExceptionObject is Exception exception)
            {
                Logger.Error("UnhandledException", exception);
            }
        }
        catch
        {
            // ignored
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
}