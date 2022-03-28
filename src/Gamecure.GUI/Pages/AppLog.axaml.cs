using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Gamecure.GUI.Pages
{
    public partial class AppLog : UserControl
    {
        public AppLog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
