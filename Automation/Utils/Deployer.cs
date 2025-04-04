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
        private readonly SettingsLoader _settingsLoader;

        public Deployer(
            SimpleShellExecutor shell,
            EnvironmentInfo environmentInfo,
            FileChecker fileChecker,
            IFileSystemWrapper ioWrapper,
            SettingsLoader settingsLoader)
        {
            _shell = shell;
            _environmentInfo = environmentInfo;
            _fileChecker = fileChecker;
            _ioWrapper = ioWrapper;
            _settingsLoader = settingsLoader;
        }

        #region EASY SCRIPT LAUNCHER
        public async Task<bool> SyncEasyScriptLauncher(string scriptsLocation)
        {
            //Check Settings
            CheckScriptLauncherSettings(scriptsLocation);

            //Check for Shortcut
            var startup = _environmentInfo.GetCommonStartupFolderPath();
            var doesShortcutExist = _ioWrapper.GetFiles(startup, "*").Any(x => x.Contains(EASY_SCRIPT_LAUNCHER, StringComparison.OrdinalIgnoreCase));
            var verifyShortcut = _shell.VerifyShortcutTarget(_ioWrapper.GetCurrentDirectory(), startup, $"{EASY_SCRIPT_LAUNCHER}.exe");

            try
            {
                if (!doesShortcutExist || !verifyShortcut)
                {
                    var files = _ioWrapper.GetFiles(startup, fileName: EASY_SCRIPT_LAUNCHER).ToArray();
                    foreach (var item in files)
                        _ioWrapper.DeleteFile(item);

                    _shell.CreateShortcut(_ioWrapper.GetCurrentDirectory(), startup, $"{EASY_SCRIPT_LAUNCHER}.exe");
                    await Task.Delay(200);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Shortcut creation failure", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        public bool CheckScriptLauncherSettings(string scriptsLocation)
        {
            var settings = $"{EASY_SCRIPT_LAUNCHER}_Settings.json";

            if (!_ioWrapper.FileExists(settings))
                _settingsLoader.LoadSettings(Path.Combine(_ioWrapper.GetCurrentDirectory(), settings));

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
                MessageBox.Show(ex.Message, settings, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }
        #endregion

        #region TASK MONITOR
        public bool SyncTaskMonitor(string scriptsLocation)
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
        #endregion
    }
}