using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Gamecure.GUI.ViewModels.FirstTimeSetup;

namespace Gamecure.GUI.Views;

public partial class FirstRunSetup : ReactiveUserControl<FirstTimeSetupViewModel>
{
    public FirstRunSetup()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}