using Automation.Utils.Helpers;
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
        private readonly EnvironmentInfo _environmentInfo;
        private readonly FileChecker _fileChecker;
        private readonly string EASY_SCRIPT_LAUNCHER = "EasyScriptLauncher";
        private readonly string TASK_MONITOR = "TaskMonitor";

        public Deployer(SimpleShellExecutor shell, EnvironmentInfo environmentInfo, FileChecker fileChecker)
        {
            _shell = shell;
            _environmentInfo = environmentInfo;
            _fileChecker = fileChecker;
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

            CheckScriptLauncherSettings(scriptsLocation);

            //Check for Shortcut
            var startupPath = _environmentInfo.GetCommonStartupFolderPath();
            var doesShortcutExist = Directory.GetFiles(startupPath, "*").Any(x => x.Contains(EASY_SCRIPT_LAUNCHER, StringComparison.OrdinalIgnoreCase));

            return doesShortcutExist;
        }

        public async Task<bool> SetupEasyScriptLauncher(string scriptsLocation, SettingsLoader scriptLoader)
        {
            var commonStartup = _environmentInfo.GetCommonStartupFolderPath();
            if (!File.Exists(Path.Combine(commonStartup, $"{EASY_SCRIPT_LAUNCHER}.lnk")))
            {
                _shell.CreateShortcut(Directory.GetCurrentDirectory(), commonStartup, EASY_SCRIPT_LAUNCHER);
                await Task.Delay(1000);
            }

            var result = await CheckEasyScriptLauncher(scriptsLocation, scriptLoader);

            return true;
        }

        public async Task<bool> UpdateEasyScriptLauncher(string scriptsLocation, SettingsLoader scriptLoader)
        {
            var settings = Path.Combine(Directory.GetCurrentDirectory(), $"{EASY_SCRIPT_LAUNCHER}_Settings.json");
            if (File.Exists(settings))
                File.Delete(settings);

            var startupPath = _environmentInfo.GetCommonStartupFolderPath();

            var shortcuts = Directory.GetFiles(startupPath, "*").Where(x => x.Contains(EASY_SCRIPT_LAUNCHER, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (shortcuts.Any())
            {
                foreach (var item in shortcuts)
                    File.Delete(item);
            }

            var monitors = Directory.GetFiles(scriptsLocation, "*").Where(x => x.Contains(TASK_MONITOR, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (monitors.Any())
            {
                foreach (var item in monitors)
                    File.Delete(item);
            }

            var resultMonitor = SetupTaskMonitor(scriptsLocation);
            var resultlauncher = await SetupEasyScriptLauncher(scriptsLocation, scriptLoader);

            return resultlauncher == resultMonitor;
        }

        public bool CheckScriptLauncherSettings(string scriptsLocation)
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

            _fileChecker.CheckFileVersion(basePath, scriptsLocation, TASK_MONITOR);

            if (string.IsNullOrEmpty(file))
                throw new Exception($"FatalError: {TASK_MONITOR} could not be found. Rebuild or download the app again!");

            var path = Path.Combine(scriptsLocation, Path.GetFileName(file));
            if (File.Exists(path))
                return true;

            return false;
        }

        public bool SetupTaskMonitor(string scriptsLocation)
        {
            var programName = TASK_MONITOR;
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