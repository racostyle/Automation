using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Automation.Windows
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        private readonly Window _parent;
        private readonly string _scriptsLocation;
        private readonly StartupLocationsHandler _startupLocationsHandler;
        private readonly EnvironmentHandler _environmentHandler;

        public DebugWindow(Window parent, string scriptsLocation, StartupLocationsHandler startupLocationsHandler, EnvironmentHandler environmentHandler)
        {
            InitializeComponent();
            _parent = parent;
            _scriptsLocation = scriptsLocation;
            _startupLocationsHandler = startupLocationsHandler;
            _environmentHandler = environmentHandler;
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
            if (Directory.Exists(_scriptsLocation))
                Process.Start("explorer.exe", _scriptsLocation);
        }

        private void tbOpenStartup_Click(object sender, RoutedEventArgs e)
        {
            if (_environmentHandler.IsSingleUser)
            {
                if (Directory.Exists(_startupLocationsHandler.GetCommonStartupFolderPath()))
                Process.Start("explorer.exe", _startupLocationsHandler.GetCommonStartupFolderPath());
            }
            else
            {
                if (Directory.Exists(_startupLocationsHandler.GetCurrentUserStartupFolder()))
                    Process.Start("explorer.exe", _startupLocationsHandler.GetCurrentUserStartupFolder());
            }
        }
    }
}
