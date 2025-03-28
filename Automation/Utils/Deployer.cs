using ConfigLib;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Automation.Utils
{
    public class Deployer
    {
        private readonly SimpleShellExecutor _shell;
        private readonly StartupLocationsHandler _startupLocationsHandler;
        private readonly string EASY_SCRIPT_LAUNCHER = "EasyScriptLauncher";
        private readonly string TASK_MONITOR = "TaskMonitor";

        public Deployer(SimpleShellExecutor shell, StartupLocationsHandler startupLocationsHandler)
        {
            _shell = shell;
            _startupLocationsHandler = startupLocationsHandler;
        }

        #region EASY SCRIPT LAUNCHER
        public async Task<bool> CheckEasyScriptLauncher(string scriptsLocation, SettingsLoader scriptLoader)
        {
            //Check Settings
            var settings = $"{EASY_SCRIPT_LAUNCHER}_Settings.json";

            if (!File.Exists(settings))
            {
                scriptLoader.LoadSettings(Path.Combine(Directory.GetCurrentDirectory(), settings));
                await Task.Delay(200);
            }

            ChangeScriptLauncherSettings(scriptsLocation);

            //Check for Shortcut
            var startupPath = _startupLocationsHandler.GetCommonStartupFolderPath();
            var doesShortcutExist = Directory.GetFiles(startupPath, "*").Any(x => x.Contains(EASY_SCRIPT_LAUNCHER, StringComparison.OrdinalIgnoreCase));

            return doesShortcutExist;
        }

        public async Task<bool> SetupEasyScriptLauncher(string scriptsLocation, EnvironmentHandler environmentHandler, SettingsLoader scriptLoader)
        {
            bool result = true;

            if (environmentHandler.IsMultiuser)
            {
                var currentUserStartup = _startupLocationsHandler.GetCurrentUserStartupFolder();
                if (!File.Exists(Path.Combine(currentUserStartup, $"{EASY_SCRIPT_LAUNCHER}.lnk")))
                    result = await CreateShortcut(scriptLoader, scriptsLocation, currentUserStartup);
            }
            else
            {
                var commonStartup = _startupLocationsHandler.GetCommonStartupFolderPath();
                if (!File.Exists(Path.Combine(commonStartup, $"{EASY_SCRIPT_LAUNCHER}.lnk")))
                    result = await CreateShortcut(scriptLoader, scriptsLocation, commonStartup);
            }
            return result;
        }

        private async Task<bool> CreateShortcut(SettingsLoader scriptLoader, string scriptsLocation, string commonStartup)
        {
            if (!File.Exists(Path.Combine(commonStartup, $"{EASY_SCRIPT_LAUNCHER}.lnk")))
            {
                _shell.CreateShortcut(Directory.GetCurrentDirectory(), commonStartup, EASY_SCRIPT_LAUNCHER);
                await Task.Delay(1000);
            }

            var result = await CheckEasyScriptLauncher(scriptsLocation, scriptLoader);
            return result;
        }

        public async Task<bool> UpdateEasyScriptLauncher(string scriptsLocation, EnvironmentHandler environmentHandler, SettingsLoader scriptLoader)
        {
            var settings = Path.Combine(Directory.GetCurrentDirectory(), $"{EASY_SCRIPT_LAUNCHER}_Settings.json");
            if (File.Exists(settings))
                File.Delete(settings);

            var monitors = Directory.GetFiles(scriptsLocation, "*").Where(x => x.Contains(TASK_MONITOR, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (monitors.Any())
            {
                foreach (var item in monitors)
                    File.Delete(item);
            }

            var resultMonitor = SetupTaskMonitor(scriptsLocation);
            var resultlauncher = await SetupEasyScriptLauncher(scriptsLocation, environmentHandler, scriptLoader);

            return resultlauncher == resultMonitor;
        }

        public bool ChangeScriptLauncherSettings(string scriptsLocation)
        {
            var settings = $"{EASY_SCRIPT_LAUNCHER}_Settings.json";

            if (!File.Exists(settings))
                return false;

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
            return true;
        }
        #endregion

        #region TASK MONITOR
        public bool CheckTaskMonitor(string scriptsLocation)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), TASK_MONITOR);
            var file = Path.GetFileName(Directory.GetFiles(basePath, "*.ps1").Where(x => x.Contains($"{TASK_MONITOR}", StringComparison.OrdinalIgnoreCase)).FirstOrDefault());

            if (string.IsNullOrEmpty(file))
                throw new Exception($"FatalError: {TASK_MONITOR} could not be found. Rebuild or download the app again!");

            var path = Path.Combine(scriptsLocation, Path.GetFileName(file));
            if (File.Exists(path))
                return true;

            return false;
        }

        public bool SetupTaskMonitor(string scriptsLocation)
        {
            var programName = "TaskMonitor";
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), programName);
            var file = Path.GetFileName(Directory.GetFiles(basePath, "*.ps1").Where(x => x.Contains($"{programName}", StringComparison.OrdinalIgnoreCase)).FirstOrDefault());

            if (string.IsNullOrEmpty(file))
                throw new Exception($"FatalError: {programName} could not be found. Rebuild or download the app again!");

            var path = Path.Combine(scriptsLocation, file);
            if (File.Exists(path))
                return true;

            try
            {
                File.Copy(Path.Combine(Path.Combine(basePath, file)), path, true);
            }
            catch
            {
                return false;
            }

            return true;
        }
        #endregion
    }
}