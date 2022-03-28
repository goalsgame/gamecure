using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Gamecure.GUI.Pages;

public partial class Quotes : UserControl
{
    public Quotes()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}