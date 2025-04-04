using Automation.Utils.Helpers;
using Automation.Utils.Helpers.Abstractions;
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
        private readonly string EASY_SCRIPT_LAUNCHER = "EasyScriptLauncher";
        private readonly string TASK_MONITOR = "TaskMonitor";

        private readonly SimpleShellExecutor _shell;
        private readonly EnvironmentInfo _environmentInfo;
        private readonly FileChecker _fileChecker;
        private readonly IFileSystemWrapper _ioWrapper;

        public Deployer(SimpleShellExecutor shell, EnvironmentInfo environmentInfo, FileChecker fileChecker, IFileSystemWrapper ioWrapper)
        {
            _shell = shell;
            _environmentInfo = environmentInfo;
            _fileChecker = fileChecker;
            _ioWrapper = ioWrapper;
        }

        #region EASY SCRIPT LAUNCHER
        public async Task<bool> CheckEasyScriptLauncher(string scriptsLocation, SettingsLoader scriptLoader)
        {
            //Check Settings
            var settings = $"{EASY_SCRIPT_LAUNCHER}_Settings.json";

            if (!_ioWrapper.FileExists(settings))
            {
                scriptLoader.LoadSettings(Path.Combine(_ioWrapper.GetCurrentDirectory(), settings));
                await Task.Delay(200);
            }

            CheckScriptLauncherSettings(scriptsLocation);

            //Check for Shortcut
            var startupPath = _environmentInfo.GetCommonStartupFolderPath();
            var doesShortcutExist = _ioWrapper.GetFiles(startupPath, "*").Any(x => x.Contains(EASY_SCRIPT_LAUNCHER, StringComparison.OrdinalIgnoreCase));

            return doesShortcutExist;
        }

        public async Task<bool> SetupEasyScriptLauncher(string scriptsLocation, SettingsLoader scriptLoader)
        {
            var commonStartup = _environmentInfo.GetCommonStartupFolderPath();
            if (!_ioWrapper.FileExists(Path.Combine(commonStartup, $"{EASY_SCRIPT_LAUNCHER}.lnk")))
            {
                _shell.CreateShortcut(_ioWrapper.GetCurrentDirectory(), commonStartup, EASY_SCRIPT_LAUNCHER);
                await Task.Delay(200);
            }

            var result = await CheckEasyScriptLauncher(scriptsLocation, scriptLoader);

            return true;
        }

        public async Task<bool> UpdateEasyScriptLauncher(string scriptsLocation, SettingsLoader scriptLoader)
        {
            var settings = Path.Combine(_ioWrapper.GetCurrentDirectory(), $"{EASY_SCRIPT_LAUNCHER}_Settings.json");
            if (_ioWrapper.FileExists(settings))
                _ioWrapper.DeleteFile(settings);

            var startupPath = _environmentInfo.GetCommonStartupFolderPath();

            var shortcuts = _ioWrapper.GetFiles(startupPath, "*")
                .Where(x => x.Contains(EASY_SCRIPT_LAUNCHER, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (shortcuts.Any())
            {
                foreach (var item in shortcuts)
                    _ioWrapper.DeleteFile(item);
            }

            var monitors = _ioWrapper.GetFiles(scriptsLocation, "*")
                .Where(x => x.Contains(TASK_MONITOR, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (monitors.Any())
            {
                foreach (var item in monitors)
                    _ioWrapper.DeleteFile(item);
            }

            var setupMonitorResult = SetupTaskMonitor(scriptsLocation);
            var resultLauncherResult = await SetupEasyScriptLauncher(scriptsLocation, scriptLoader);

            return resultLauncherResult == setupMonitorResult;
        }

        public bool CheckScriptLauncherSettings(string scriptsLocation)
        {
            var settings = $"{EASY_SCRIPT_LAUNCHER}_Settings.json";

            if (!_ioWrapper.FileExists(settings))
                return false;

            try
            {
                var text = _ioWrapper.ReadAllText(settings);
                var config = JsonSerializer.Deserialize<Config>(text);
                config.ScriptsFolder = scriptsLocation;

                text = JsonSerializer.Serialize(config);
                _ioWrapper.WriteAllText(settings, text);
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
            var sourceTaskMonitorPath = Path.Combine(_ioWrapper.GetCurrentDirectory(), TASK_MONITOR);
            var file = Path.GetFileName(_ioWrapper.GetFiles(sourceTaskMonitorPath, "*.ps1")
                .Where(x => x.Contains($"{TASK_MONITOR}", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault());

            if (string.IsNullOrEmpty(file))
                throw new Exception($"FatalError: {TASK_MONITOR} could not be found. Rebuild or download the app again!");

            var syncResult = _fileChecker.SyncLatestFileVersion(sourceTaskMonitorPath, scriptsLocation, $"{TASK_MONITOR}.ps1");
            var deployedScriptPath = Path.Combine(scriptsLocation, Path.GetFileName(file));

            return _ioWrapper.FileExists(deployedScriptPath) && syncResult;
        }

        public bool SetupTaskMonitor(string scriptsLocation)
        {
            return CheckTaskMonitor(scriptsLocation);
        }
        #endregion
    }
}