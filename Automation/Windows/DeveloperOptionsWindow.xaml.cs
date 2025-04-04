using Automation.Utils;
using Automation.Utils.Helpers;
using System.Diagnostics;
using System.Windows;

namespace Automation
{
    /// <summary>
    /// Interaction logic for DeveloperOptions.xaml
    /// </summary>
    public partial class DeveloperOptionsWindow : Window
    {
        private readonly Deployer _deployer;
        private readonly EnvironmentInfo _environmentInfo;
        private readonly string _scriptsLocation;

        public DeveloperOptionsWindow(Deployer deployer, EnvironmentInfo environmentInfo, string scriptsLocation)
        {
            InitializeComponent();
            _deployer = deployer;
            _environmentInfo = environmentInfo;
            _scriptsLocation = scriptsLocation;
            HideOverlay();
        }

        private async void OnBtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (cbhDoUpdate.IsChecked == false)
                return;

            ShowOverlay();
            var result = await _deployer.SyncEasyScriptLauncher(_scriptsLocation);
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
            process.StartInfo.Arguments = _environmentInfo.GetCommonStartupFolderPath();
            process.Start();
        }
    }
}
