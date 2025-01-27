using Automation.ConfigurationAdapter;
using Microsoft.Windows.Themes;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Automation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly LoadedConfigsComboBoxWrapper _configsWrapper;
        private readonly VisualTreeAdapter _visualTreeAdapter;

        public MainWindow()
        {
            InitializeComponent();

            _configsWrapper = new LoadedConfigsComboBoxWrapper(cbbConfigs);
            _visualTreeAdapter = BuildHandler();

            this.tbScriptsLocation.TextChanged += OnTbScriptsLocation_TextChanged;
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
        }

        private VisualTreeAdapter BuildHandler()
        {
            var handlers = new IVisualHandler[]
            {
                 new Handler_TextBox(),
                 new Handler_CheckBox()
            };

            return new VisualTreeAdapter(handlers);
        }

        #region CLOSED & LOADED HANDLERS
        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            var config = _visualTreeAdapter.Pack(this);

            var text = JsonSerializer.Serialize(config);
            File.WriteAllText("appsettings.json", text);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Delay(100);

            if (!File.Exists("appsettings.json"))
                return;

            var json = File.ReadAllText("appsettings.json");
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            _visualTreeAdapter.Unpack(this, config);

            InitAfterLoaded();

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

        #endregion

        private void OnBtnNewAutomationScript_Click(object sender, RoutedEventArgs e)
        {
            Window_TaskMonitor_Config secWindow = new Window_TaskMonitor_Config(_visualTreeAdapter, tbScriptsLocation.Text, string.Empty);
            secWindow.ShowDialog();

            _configsWrapper.Load(tbScriptsLocation.Text);
        }
    }

    internal class LoadedConfigsComboBoxWrapper
    {
        private readonly ComboBox _comboBox;

        public LoadedConfigsComboBoxWrapper(ComboBox comboBox)
        {
            _comboBox = comboBox;
        }

        internal void Load(string location)
        {
            _comboBox.Items.Clear();

            var config = Directory.GetFiles(location, "*_Config.json");

            if (!config.Any())
                return;

            foreach (var cfg in config)
            {
                _comboBox.Items.Add(cfg);
            }
        }

        internal string GetValue()
        {
            if (_comboBox.Items.Count == 0)
                return string.Empty;
            return _comboBox.Text;
        }
    }
}