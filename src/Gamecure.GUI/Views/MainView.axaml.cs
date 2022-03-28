using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Gamecure.GUI.ViewModels;

namespace Gamecure.GUI.Views;

public partial class MainView : ReactiveUserControl<MainViewModel>
{
    public MainView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}