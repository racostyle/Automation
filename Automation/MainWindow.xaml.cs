using Automation.ConfigurationAdapter;
using Automation.Utils;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;

namespace Automation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ComboBoxWrapper_TaskMonitorConfigs _configsWrapper;
        private readonly VisualTreeAdapter _visualTreeAdapter;
        private readonly Deployer _deployer;

        public MainWindow()
        {
            InitializeComponent();

            _configsWrapper = new ComboBoxWrapper_TaskMonitorConfigs(cbbConfigs);

            _visualTreeAdapter = new VisualTreeAdapterBuilder()
                .Configure_HandleTextBox()
                .Configure_HandleCheckBox()
                .Build();

            _deployer = new Deployer(new SimpleShellExecutor());
        }

        #region CLOSED & LOADED HANDLERS
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!File.Exists("appsettings.json"))
                return;

            var json = File.ReadAllText("appsettings.json");
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            _visualTreeAdapter.Unpack(this, config);

            await InitAfterLoadedAsync();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            var config = _visualTreeAdapter.Pack(this);

            var text = JsonSerializer.Serialize(config);
            File.WriteAllText("appsettings.json", text);
        }

        private async Task InitAfterLoadedAsync()
        {
            if (string.IsNullOrEmpty(tbScriptsLocation.Text))
            {
                tbScriptsLocation.Text = "C:\\Delivery\\Automation\\Scripts";
                Directory.CreateDirectory(tbScriptsLocation.Text);
            }

            var result = await _deployer.CheckEasyScriptLauncher(tbScriptsLocation.Text);
            if (result)
                btnSetupScripLauncher.Background = Brushes.DarkGreen;
            else
                btnSetupScripLauncher.Background = Brushes.DarkRed;

            result = _deployer.CheckTaskMonitor(tbScriptsLocation.Text);
            if (result)
                btnSetupTaskMonitor.Background = Brushes.DarkGreen;
            else
                btnSetupTaskMonitor.Background = Brushes.DarkRed;
        }

        #endregion

        #region auto handlers
        private void OnTbScriptsLocation_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!BaseScriptLocationSafetyCheck())
                return;

            _configsWrapper.Load(tbScriptsLocation.Text);
        }

        #endregion

        #region Buttons
        private void OnBtnEditAutomationScript_Click(object sender, RoutedEventArgs e)
        {
            if (!BaseScriptLocationSafetyCheck())
                return;

            var configLocation = _configsWrapper.GetValue();
            if (string.IsNullOrEmpty(configLocation))
                return;

            Window_TaskMonitor_Config secWindow = new Window_TaskMonitor_Config(_visualTreeAdapter, tbScriptsLocation.Text, configLocation);
            secWindow.ShowDialog();

        }

        private void OnBtnNewAutomationScript_Click(object sender, RoutedEventArgs e)
        {
            if (!BaseScriptLocationSafetyCheck())
                return;

            Window_TaskMonitor_Config secWindow = new Window_TaskMonitor_Config(_visualTreeAdapter, tbScriptsLocation.Text, string.Empty);
            secWindow.ShowDialog();

            _configsWrapper.Load(tbScriptsLocation.Text);
        }

        private async void OnBtnSetupScripLauncher_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlay();
            var scriptLauncher = "EasyScriptLauncher";
            var result = await _deployer.SetupEasyScriptLauncher(Directory.GetCurrentDirectory(), scriptLauncher, tbScriptsLocation.Text);
            if (result)
                btnSetupScripLauncher.Background = Brushes.DarkGreen;
            else
                btnSetupScripLauncher.Background = Brushes.DarkRed;
            HideOverlay();
        }

        private void OnBtnSetupTaskMonitor_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlay();
            if (!BaseScriptLocationSafetyCheck())
                return;

            var result = _deployer.SetupTaskMonitor(tbScriptsLocation.Text);
            if (result)
                btnSetupTaskMonitor.Background = Brushes.DarkGreen;
            else
                btnSetupTaskMonitor.Background = Brushes.DarkRed;
            HideOverlay();
        }

        #endregion

        #region Auxiliary
        private bool BaseScriptLocationSafetyCheck()
        {
            if (!Directory.Exists(tbScriptsLocation.Text))
            {
                MessageBox.Show("Base script location not set or does not exist!");
                return false;
            }
            return true;
        }

        private void ShowOverlay()
        {
            recOverlay.Visibility = Visibility.Visible;
        }

        private void HideOverlay()
        {
            recOverlay.Visibility = Visibility.Hidden;
        }
        #endregion
    }
}
