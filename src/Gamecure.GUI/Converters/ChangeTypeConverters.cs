using Avalonia.Data.Converters;
using Avalonia.Media;
using Gamecure.Core.Plastic.Parser;

namespace Gamecure.GUI.Converters;

public static class ChangeTypeConverters
{
    private static readonly SolidColorBrush White = new(0xffffffff);
    private static readonly SolidColorBrush Deleted = new(0xffff0000);
    private static readonly SolidColorBrush Changed = new(0xffffff00);
    private static readonly SolidColorBrush Added = new(0xff00ffff);
    private static readonly SolidColorBrush Moved = new(0xffff00ff);

    public static readonly IValueConverter TypeToBrush = new FuncValueConverter<PlasticChangeType, SolidColorBrush>(type => type switch
    {
        PlasticChangeType.Added => Added,
        PlasticChangeType.Changed => Changed,
        PlasticChangeType.Deleted => Deleted,
        PlasticChangeType.Moved => Moved,
        _ => White
    });
}