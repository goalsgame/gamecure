using System;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Gamecure.Core.Common.Logging;

namespace Gamecure.GUI.Converters;

public static class LogLevelConverters
{
    private static readonly SolidColorBrush Black = new(0xff000000);
    private static readonly SolidColorBrush Trace = new(0xff00ffff);
    private static readonly SolidColorBrush Info = new(0xff008989);
    private static readonly SolidColorBrush Warning = new(0xffffff00);
    private static readonly SolidColorBrush Error = new(0xffff0000);

    public static readonly IValueConverter LevelToBrush = new FuncValueConverter<string, SolidColorBrush>(s =>
    {
        if (Enum.TryParse(s, true, out LogLevel level))
        {
            return level switch
            {
                LogLevel.Debug or LogLevel.Trace => Trace,
                LogLevel.Error => Error,
                LogLevel.Info => Info,
                LogLevel.Warning => Warning,
                _ => Black
            };
        }
        return Black;
    });
}