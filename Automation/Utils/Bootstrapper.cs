using ConfigLib;
using System.IO;
using System;
using System.Windows.Controls;
using System.Windows;
using System.Threading.Tasks;

namespace Automation.Utils
{
    internal class Bootstrapper
    {
        private MainWindow _mainWindow;
        private SettingsHandler _settingsHandler;
        private readonly Deployer _deployer;
        private readonly TextBox _tbScriptsLocation;
        private string _scriptsLocation;
        private Button _btnSetupScripLauncher;
        private Button _btnSetupTaskMonitor;
        private SettingsLoader _settingsLoader;
        private readonly Action<bool, Button> _colorButtonAction;

        public Bootstrapper(
            MainWindow mainWindow, 
            SettingsHandler settingsHandler, 
            Deployer deployer,
            TextBox tbScriptsLocation,
            Button btnSetupScripLauncher, 
            Button btnSetupTaskMonitor, 
            SettingsLoader settingsLoader,
            Action<bool, Button> colorButton)
        {
            _mainWindow = mainWindow;
            _settingsHandler = settingsHandler;
            _deployer = deployer;
            _tbScriptsLocation = tbScriptsLocation;
            _btnSetupScripLauncher = btnSetupScripLauncher;
            _btnSetupTaskMonitor = btnSetupTaskMonitor;
            _settingsLoader = settingsLoader;
            _colorButtonAction = colorButton;
        }

        internal async Task Init()
        {
            if (File.Exists(_settingsHandler.SETTINGS))
            {
                var config = _settingsHandler.Unpack(_mainWindow);
                if (config.TryGetValue("ScriptsLocation", out var location))
                    _scriptsLocation = location;
                else
                    _scriptsLocation = _settingsHandler.GetDefaultScriptsLocation();
            }
            else
            {
                _scriptsLocation = _settingsHandler.GetDefaultScriptsLocation();
                _tbScriptsLocation.Text = _scriptsLocation;
                _settingsHandler.Pack(_mainWindow);
            }

            if (!DoesScriptsLocationExist(_scriptsLocation))
                SetupEnvironment();

            var result = await _deployer.CheckEasyScriptLauncher(_scriptsLocation, _settingsLoader);
            _colorButtonAction.Invoke(result, _btnSetupScripLauncher);

            result = _deployer.CheckTaskMonitor(_scriptsLocation);
            _colorButtonAction.Invoke(result, _btnSetupTaskMonitor);
        }

        private bool DoesScriptsLocationExist(string location)
        {
            bool result = true;
            if (string.IsNullOrEmpty(location))
                result = false;
            if (!Directory.Exists(location))
                result = false;
            return result;
        }

        private void SetupEnvironment()
        {
            if (!Directory.Exists(_scriptsLocation))
            {
                Directory.CreateDirectory(_scriptsLocation);
                MessageBox.Show($"Directory '{_scriptsLocation}' was created");
            }

            var recurringPath = Path.Combine(_scriptsLocation, _settingsHandler.RECURRING_SCRIPTS_LOCATION);
            if (!Directory.Exists(recurringPath))
            {
                Directory.CreateDirectory(recurringPath);
                MessageBox.Show($"Directory '{recurringPath}' was created");
            }

            try
            {
                _deployer.CheckScriptLauncherSettings(_scriptsLocation);
                if (!_settingsHandler.Pack(_mainWindow))
                    MessageBox.Show("Could not create config!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("EasyScriptLauncher_Settings.json could not be saved!" + Environment.NewLine + ex.Message);
            }
        }
    }
}
