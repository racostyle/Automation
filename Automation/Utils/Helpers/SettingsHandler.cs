using Automation.ConfigurationAdapter;
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

        public SettingsHandler(VisualTreeAdapter adapter)
        {
            VisualTreeAdapter = adapter;
        }

        //internal string GetCurrentUserScriptLocation()
        //{
        //    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), SCRIPT_FOLDER);
        //}

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
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        internal Dictionary<string, string> Unpack(MainWindow window)
        {
            var json = File.ReadAllText(SETTINGS);
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            VisualTreeAdapter.Unpack(window, config);

            return config;
        }
    }
}
