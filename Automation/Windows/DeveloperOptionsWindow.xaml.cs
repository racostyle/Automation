using Automation.Utils;
using System.Diagnostics;
using System.Windows;

namespace Automation
{
    /// <summary>
    /// Interaction logic for DeveloperOptions.xaml
    /// </summary>
    public partial class DeveloperOptionsWindow : Window
    {
        private readonly DeployHandler _deployer;
        private readonly string _scriptsLocation;
        private readonly EnvironmentHandler _environmentHandler;
        private readonly StartupLocationsHandler _startupLocationsHandler;
        private readonly string _environmentType;

        public DeveloperOptionsWindow(DeployHandler deployer, string scriptsLocation, EnvironmentHandler environmentHandler, StartupLocationsHandler startupLocationsHandler)
        {
            InitializeComponent();
            _deployer = deployer;
            _scriptsLocation = scriptsLocation;
            _environmentHandler = environmentHandler;
            _startupLocationsHandler = startupLocationsHandler;
            HideOverlay();
        }

        private async void OnBtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (cbhDoUpdate.IsChecked == false)
                return;

            ShowOverlay();
            var result = await _deployer.UpdateEasyScriptLauncher(_scriptsLocation, _environmentHandler, new ConfigLib.SettingsLoader());
            cbhDoUpdate.IsChecked = false;
            HideOverlay();
        }

        private void ShowOverlay()
        {
            recOverlay.Visibility = Visibility.Visible;
        }

        private void HideOverlay()
        {
            recOverlay.Visibility = Visibility.Hidden;
        }

        private void OnBtnOpenConfigsLocation_Click(object sender, RoutedEventArgs e)
        {
            var process = new Process();
            process.StartInfo.FileName = "explorer.exe";
            process.StartInfo.Arguments = _scriptsLocation;
            process.Start();
        }

        private void OnBtnOpenStartupLocation_Click(object sender, RoutedEventArgs e)
        {
            var process = new Process();
            process.StartInfo.FileName = "explorer.exe";
            process.StartInfo.Arguments = _startupLocationsHandler.GetCommonStartupFolderPath();
            process.Start();
        }
    }
}
