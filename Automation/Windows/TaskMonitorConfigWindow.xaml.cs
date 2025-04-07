using Automation.ConfigurationAdapter;
using Automation.Utils.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace Automation
{
    /// <summary>
    /// Interaction logic for TaskMonitorConfigWindow.xaml
    /// </summary>
    public partial class TaskMonitorConfigWindow : Window
    {
        private readonly VisualTreeAdapter _visualTreeAdapter;
        private readonly string _baseScriptsLocation;
        private string _configLocation;
        private string _fileName;
        public string FileName => _fileName;

        public TaskMonitorConfigWindow(VisualTreeAdapter visualTreeAdapter, string baseScriptsLocation = "", string configLocation = "")
        {
            InitializeComponent();
            _visualTreeAdapter = visualTreeAdapter;
            _baseScriptsLocation = baseScriptsLocation;
            _configLocation = configLocation;

            this.Loaded += Window_TaskMonitor_Config_Loaded;
        }

        private void Window_TaskMonitor_Config_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_configLocation))
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
            RepairConfigValues();
        }

        private void RepairConfigValues()
        {
            if (string.IsNullOrEmpty(tbPriority.Text) || !int.TryParse(tbPriority.Text, out var _))
            {
                tbPriority.Text = "100";
            }
            if (string.IsNullOrEmpty(tbInterval.Text) || !int.TryParse(tbPriority.Text, out var _))
            {
                tbInterval.Text = "1";
            }
        }

        private void OnBtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void OnBtnConfirmAndSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbExecutableName.Text))
            {
                MessageBox.Show("\tError: invalid path!\t");
                return;
            }

            _fileName = $"{Path.GetFileNameWithoutExtension(tbExecutableName.Text)}_Config.json";
            _configLocation = Path.Combine(_baseScriptsLocation, _fileName);

            var config = _visualTreeAdapter.Pack(this);
            var json = JsonSerializer.Serialize(config);
            File.WriteAllText(_configLocation, json);

            this.DialogResult = true;
            this.Close();
        }

        private void OnBtnSearchForLocation_Click(object sender, RoutedEventArgs e)
        {
            var file = new FolderDialogWrapper().ShowFileDialog_ReturnPath();
            if (string.IsNullOrEmpty(file))
                return;

            if (!File.Exists(file))
                return;

            tbBaseFolder.Text = Path.GetDirectoryName(file);
            tbExecutableName.Text = Path.GetFileName(file);
        }

        private void OnNumericTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }
    }
}
