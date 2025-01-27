using Automation.ConfigurationAdapter;
using Automation.Utils;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace Automation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ComboBoxWrapper_TaskMonitorConfigs _configsWrapper;
        private readonly VisualTreeAdapter _visualTreeAdapter;

        public MainWindow()
        {
            InitializeComponent();

            _configsWrapper = new ComboBoxWrapper_TaskMonitorConfigs(cbbConfigs);

            _visualTreeAdapter = new VisualTreeAdapterBuilder()
                .Configure_HandleTextBox()
                .Configure_HandleCheckBox()
                .Build();
        }

        #region CLOSED & LOADED HANDLERS
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!File.Exists("appsettings.json"))
                return;

            var json = File.ReadAllText("appsettings.json");
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            _visualTreeAdapter.Unpack(this, config);

            InitAfterLoaded();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            var config = _visualTreeAdapter.Pack(this);

            var text = JsonSerializer.Serialize(config);
            File.WriteAllText("appsettings.json", text);
        }

        private void InitAfterLoaded()
        {
            if (string.IsNullOrEmpty(tbScriptsLocation.Text))
                tbScriptsLocation.Text = "C:\\Delivery\\Automation\\Scripts";
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
        private void OnBtnSearchScriptLocation_Click(object sender, RoutedEventArgs e)
        {
            var location = new FolderDialogWrapper().ShowFolderDialog_ReturnPath();

            if (!string.IsNullOrEmpty(location))
                tbScriptsLocation.Text = location;

        }

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
            Window_TaskMonitor_Config secWindow = new Window_TaskMonitor_Config(_visualTreeAdapter, tbScriptsLocation.Text, string.Empty);
            secWindow.ShowDialog();

            _configsWrapper.Load(tbScriptsLocation.Text);
        }


        #endregion

       
    }
}