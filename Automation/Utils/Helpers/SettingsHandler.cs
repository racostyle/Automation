using Automation.ConfigurationAdapter;
using Automation.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Automation.Utils.Helpers
{
    internal class SettingsHandler
    {
        internal readonly string DEFAULT_SCRIPTS_LOCATION = "C:\\Delivery\\";
        internal readonly string SCRIPT_FOLDER = "Automation\\Scripts";
        internal readonly string RECURRING_SCRIPTS_LOCATION = "RecurringScripts";
        internal readonly string SETTINGS = "appsettings.json";

        internal readonly VisualTreeAdapter VisualTreeAdapter;
        private readonly ILogger _logger;

        public SettingsHandler(VisualTreeAdapter adapter, ILogger logger)
        {
            VisualTreeAdapter = adapter;
            _logger = logger;
        }

        internal string GetDefaultScriptsLocation()
        {
            return Path.Combine(DEFAULT_SCRIPTS_LOCATION, SCRIPT_FOLDER);
        }

        internal bool Pack(MainWindow window)
        {
            var config = VisualTreeAdapter.Pack(window);

            var text = JsonSerializer.Serialize(config);
            try
            {
                File.WriteAllText(SETTINGS, text);
                _logger?.Log($"Config {SETTINGS} saved");
            }
            catch (Exception ex)
            {
                _logger?.Log($"Failed to save Config {SETTINGS}. Error: {ex.Message}");
                return false;
            }
            return true;
        }

        internal Dictionary<string, string> Unpack(MainWindow window)
        {
            Dictionary<string, string> config;
            try
            {
                var json = File.ReadAllText(SETTINGS);
                config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                VisualTreeAdapter.Unpack(window, config);
                _logger?.Log($"Config {SETTINGS} unpacked successfully");
                return config;
            }
            catch (Exception ex)
            {
                _logger?.Log($"Config {SETTINGS} couldn't be processed! Error: {ex.Message}");
                return new Dictionary<string, string>();
            }
        }
    }
}
