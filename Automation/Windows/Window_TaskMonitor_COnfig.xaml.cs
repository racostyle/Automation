using Automation.ConfigurationAdapter;
using Automation.Utils;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace Automation
{
    /// <summary>
    /// Interaction logic for Window_TaskMonitor_Config.xaml
    /// </summary>
    public partial class Window_TaskMonitor_Config : Window
    {
        private readonly VisualTreeAdapter _visualTreeAdapter;
        private readonly string _baseScriptsLocation;
        private string _configLocation;

        public Window_TaskMonitor_Config(VisualTreeAdapter visualTreeAdapter, string baseScriptsLocation = "", string configLocation = "")
        {
            InitializeComponent();
            _visualTreeAdapter = visualTreeAdapter;
            _baseScriptsLocation = baseScriptsLocation;
            _configLocation = configLocation;

            if (string.IsNullOrEmpty(_configLocation))
                return;

            this.Loaded += Window_TaskMonitor_Config_LoadedAsync;
        }

        private void Window_TaskMonitor_Config_LoadedAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                var text = File.ReadAllText(_configLocation);
                var json = JsonSerializer.Deserialize<Dictionary<string, string>>(text);

                _visualTreeAdapter.Unpack(this, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void tbCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void tbConfirmAndSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbExecutableName.Text))
            {
                MessageBox.Show("\tError: invalid path!\t");
                return;
            }

            var fileName = $"{Path.GetFileNameWithoutExtension(tbExecutableName.Text)}_Config.json";
            _configLocation = Path.Combine(_baseScriptsLocation, fileName);

            var config = _visualTreeAdapter.Pack(this);
            var json = JsonSerializer.Serialize(config);
            File.WriteAllText(_configLocation, json);

            this.DialogResult = false;
            this.Close();
        }

        private void btnSearchForLocation_Click(object sender, RoutedEventArgs e)
        {
            var file = new FolderDialogWrapper().ShowFileDialog_ReturnPath();
            if (string.IsNullOrEmpty(file))
                return;

            if (!File.Exists(file))
                return;

            tbBaseFolder.Text = Path.GetDirectoryName(file);
            tbExecutableName.Text = Path.GetFileName(file);
        }
    }
}
