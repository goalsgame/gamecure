using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Gamecure.GUI.ViewModels;

namespace Gamecure.GUI.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}