using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Gamecure.GUI.Pages;

public partial class Config : UserControl
{
    public Config()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}