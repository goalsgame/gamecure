using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Gamecure.GUI.Pages
{
    public partial class EditorSync : UserControl
    {
        public EditorSync()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
