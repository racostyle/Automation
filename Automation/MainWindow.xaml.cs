using Automation.ConfigurationAdapter;
using Automation.Utils;
using ConfigLib;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Documents;
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
            if (Directory.Exists(tbScriptsLocation.Text))
            {
                _configsWrapper.Load(tbScriptsLocation.Text);
            }
        }

        #endregion

        #region Buttons
        private void OnBtnEditAutomationScript_Click(object sender, RoutedEventArgs e)
        {
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

        private void OnBtnSetupScripLauncher_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OnBtnSetupTaskMonitor_Click(object sender, RoutedEventArgs e)
        {

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
        #endregion
    }

    public class Deployer
    {
        private readonly SimpleShellExecutor _shell;

        public Deployer(SimpleShellExecutor shell)
        {
            _shell = shell;
        }

        public async Task<bool> CheckEasyScriptLauncher(string scriptsLocation)
        {
            //Check Settings
            var name = "EasyScriptLauncher";
            var settings = $"{name}_Settings.json";

            if (!File.Exists(settings))
            {
                var process = _shell.ExecuteExe(Path.Combine(Directory.GetCurrentDirectory(), $"{name}.exe"));
                await Task.Delay(2000);
                process?.Kill();
            }

            try
            {
                var text = File.ReadAllText(settings);
                var config = JsonSerializer.Deserialize<Config>(text);
                config.ScriptsFolder = scriptsLocation;

                text = JsonSerializer.Serialize(config);
                File.WriteAllText(settings, text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, settings);
                return false;
            }

            //Check for Shortcut
            var startupPath = GetCommonStartupFolderPathManual();
            var doesShortcutExist = Directory.GetFiles(startupPath, "*").Any(x => x.Contains(name, StringComparison.OrdinalIgnoreCase));

            return doesShortcutExist;
        }

        public static string GetCommonStartupFolderPathManual()
        {
            var programData = Environment.GetEnvironmentVariable("ProgramData");
            var commonStartupPath = Path.Combine(programData, @"Microsoft\Windows\Start Menu\Programs\Startup");
            return commonStartupPath;
        }

        public void SetupEasyScriptLauncher(string location)
        {

        }

        public bool CheckTaskMonitor(string scriptsLocation)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "TaskMonitor");
            var file = Path.GetFileName(Directory.GetFiles(basePath, "*.ps1").FirstOrDefault());

            if (string.IsNullOrEmpty(file))
                throw new Exception("FatalError: TaskMonitor could not be found. Rebuild or download the app again!");

            var jebemtigovna = Path.Combine(scriptsLocation, Path.GetFileName(file));
            if (File.Exists(jebemtigovna))
                return true;

            return false;
        }

        public void SetupTaskMonitor(string scriptsLocation)
        {

        }
    }
}