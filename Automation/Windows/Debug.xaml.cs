using System.Windows;

namespace Automation.Windows
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        private readonly Window _parent;

        public DebugWindow(Window parent)
        {
            InitializeComponent();
            _parent = parent;

            _parent.LocationChanged += OnParent_LocationChanged;
            SetPosition();
        }

        private void OnParent_LocationChanged(object? sender, EventArgs e)
        {
            SetPosition();
        }

        private void SetPosition()
        {
            this.Left = _parent.Left + _parent.Width;
            this.Top = _parent.Top;
        }

        private void tbOpenScripts_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void tbOpenStartup_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
