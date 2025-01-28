using Automation.ConfigurationAdapter;
using Automation.Utils;
using System.IO;
using System.Text;
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
            var scriptLauncher = "EasyScriptLauncher";
            var result = await _deployer.SetupEasyScriptLauncher(Directory.GetCurrentDirectory(), scriptLauncher, tbScriptsLocation.Text);
            if (result)
                btnSetupScripLauncher.Background = Brushes.DarkGreen;
            else
                btnSetupScripLauncher.Background = Brushes.DarkRed;
        }

        private void OnBtnSetupTaskMonitor_Click(object sender, RoutedEventArgs e)
        {
            if (!BaseScriptLocationSafetyCheck())
                return;

            var result = _deployer.SetupTaskMonitor(tbScriptsLocation.Text);
            if (result)
                btnSetupTaskMonitor.Background = Brushes.DarkGreen;
            else
                btnSetupTaskMonitor.Background = Brushes.DarkRed;
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

    public class ScriptEditor
    {
        private readonly string _filePath;
        private string _tempFile = Path.GetTempFileName();

        public ScriptEditor(string filePath)
        {
            _filePath = filePath;
        }

        public void EditDelayTime(int newDelayInSeconds)
        {
            try
            {
                // Read the encoding of the original file
                Encoding encoding = GetEncoding(_filePath);

                using (var sr = new StreamReader(_filePath, encoding))
                using (var sw = new StreamWriter(_tempFile, false, encoding))
                {
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Trim().Contains("#Delay before loop", StringComparison.OrdinalIgnoreCase))
                        {
                            line = $"Start-Sleep -Seconds {newDelayInSeconds} #Delay before loop";
                        }
                        sw.WriteLine(line);
                    }
                }

                // Replace the original file with the modified file
                File.Delete(_filePath);
                File.Move(_tempFile, _filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
        private Encoding GetEncoding(string filePath)
        {
            using (var reader = new StreamReader(filePath, true))
            {
                reader.Peek(); // you need to do an operation to force the StreamReader to detect the encoding
                return reader.CurrentEncoding;
            }
        }
    }
}
