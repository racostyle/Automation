using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace Automation.Utils
{
    internal class ComboBoxWrapper_TaskMonitorConfigs
    {
        private readonly ComboBox _comboBox;

        public ComboBoxWrapper_TaskMonitorConfigs(ComboBox comboBox)
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

            _comboBox.SelectedIndex = 0;
        }

        internal string GetValue()
        {
            if (_comboBox.Items.Count == 0)
                return string.Empty;

            return _comboBox.Text;
        }
    }
}