using Automation.ConfigurationAdapter;
using Automation.Utils;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
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
            if (!Environment.IsPrivilegedProcess)
            {
                MessageBox.Show("Start this app as admin!");
                Environment.Exit(0);
            }

            InitializeComponent();

            _configsWrapper = new ComboBoxWrapper_TaskMonitorConfigs(cbbConfigs);

            _visualTreeAdapter = new VisualTreeAdapterBuilder()
                .Add_HandlerTextBox()
                .Add_HandlerCheckBox()
                .Build();

            _deployer = new Deployer(new SimpleShellExecutor());

            
        }

        #region CLOSED & LOADED HANDLERS
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists("appsettings.json"))
            {
                var json = File.ReadAllText("appsettings.json");
                var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                _visualTreeAdapter.Unpack(this, config);

                var result = await _deployer.CheckEasyScriptLauncher(tbScriptsLocation.Text);
                ColorButton(result, btnSetupScripLauncher);

                result = _deployer.CheckTaskMonitor(tbScriptsLocation.Text);
                ColorButton(result, btnSetupTaskMonitor);

                cbhDoUpdate.IsChecked = false;

                HideOverlay();
            }

            if (!CheckScriptsLocation())
            {
                tbScriptsLocation.Text = "C:\\Delivery\\Automation\\Scripts";
                Directory.CreateDirectory(tbScriptsLocation.Text);
                SaveConfig();
            }

            LoadConfigs();

            HideOverlay();
        }

        private bool CheckScriptsLocation()
        {
            if (string.IsNullOrEmpty(tbScriptsLocation.Text))
                return false;
            if (!Directory.Exists(tbScriptsLocation.Text))
                return false;
            return true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void SaveConfig()
        {
            var config = _visualTreeAdapter.Pack(this);

            var text = JsonSerializer.Serialize(config);
            File.WriteAllText("appsettings.json", text);
        }

        #endregion

        #region BUTTONS
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
            var result = await _deployer.SetupEasyScriptLauncher(tbScriptsLocation.Text);
            ColorButton(result, btnSetupScripLauncher);
            HideOverlay();
        }

        private void OnBtnSetupTaskMonitor_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlay();
            if (!BaseScriptLocationSafetyCheck())
                return;

            var result = _deployer.SetupTaskMonitor(tbScriptsLocation.Text);
            ColorButton(result, btnSetupTaskMonitor);
            HideOverlay();
        }

        
        private async void OnBtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (cbhDoUpdate.IsChecked == false)
                return;

            ShowOverlay();
            var result = await _deployer.UpdateEasyScriptLauncher(tbScriptsLocation.Text);
            ColorButton(result, btnSetupTaskMonitor);
            ColorButton(result, btnSetupScripLauncher);
            cbhDoUpdate.IsChecked = false;
            HideOverlay();
        }

        private void OnBtnLoadScripts_Click(object sender, RoutedEventArgs e)
        {
            LoadConfigs();
        }
        #endregion

        #region AUXILIARY
        private bool BaseScriptLocationSafetyCheck()
        {
            if (!Directory.Exists(tbScriptsLocation.Text))
            {
                MessageBox.Show("Base script location not set or does not exist!");
                return false;
            }
            return true;
        }

        private void LoadConfigs()
        {
            if (!BaseScriptLocationSafetyCheck())
                return;

            _configsWrapper.Load(tbScriptsLocation.Text);
        }

        private void ColorButton(bool result, Button button)
        {
            if (result)
                button.Background = Brushes.DarkGreen;
            else
                button.Background = Brushes.DarkRed;
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
