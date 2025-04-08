using Automation.Logging;
using Automation.Utils.Helpers.Abstractions;
using Automation.Utils.Helpers.FileCheck;
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
        public readonly string EASY_SCRIPT_LAUNCHER = "EasyScriptLauncher";
        public readonly string TASK_MONITOR = "TaskMonitor";

        private readonly ILogger _logger;
        private readonly ISimpleShellExecutor _shell;
        private readonly IEnvironmentInfo _environmentInfo;
        private readonly IFileChecker _fileChecker;
        private readonly IFileSystemWrapper _ioWrapper;
        private readonly ISettingsLoader _settingsLoader;
        private readonly IMessageBoxWrapper _messageBoxWrapper;

        public Deployer(
            ILogger logger,
            ISimpleShellExecutor shell,
            IEnvironmentInfo environmentInfo,
            IFileChecker fileChecker,
            IFileSystemWrapper ioWrapper,
            ISettingsLoader settingsLoader,
            IMessageBoxWrapper messageBoxWrapper)
        {
            _logger = logger;
            _shell = shell;
            _environmentInfo = environmentInfo;
            _fileChecker = fileChecker;
            _ioWrapper = ioWrapper;
            _settingsLoader = settingsLoader;
            _messageBoxWrapper = messageBoxWrapper;
        }

        #region EASY SCRIPT LAUNCHER
        public async Task<bool> SyncEasyScriptLauncher(string scriptsLocation)
        {
            //Check Settings
            var settingsResult = CheckScriptLauncherSettings(scriptsLocation);
            if (!settingsResult)
                return false;

            //Check for Shortcut
            var startup = _environmentInfo.GetCommonStartupFolderPath();
            var fls = _ioWrapper.GetFiles(startup);
            var doesShortcutExist = _ioWrapper.GetFiles(startup).Any(x => x.Contains(EASY_SCRIPT_LAUNCHER, StringComparison.OrdinalIgnoreCase));
            var verifyShortcut = _shell.VerifyShortcutTarget(_ioWrapper.GetCurrentDirectory(), startup, $"{EASY_SCRIPT_LAUNCHER}.exe");

            try
            {
                var exe = Path.Combine(_ioWrapper.GetCurrentDirectory(), $"{EASY_SCRIPT_LAUNCHER}.exe");

                if (!doesShortcutExist || !verifyShortcut)
                {
                    _logger?.Log($"Shortcut does not exist or pointing to wrong target!");
                    var files = _ioWrapper.GetFiles(startup, fileName: EASY_SCRIPT_LAUNCHER).ToArray();
                    foreach (var item in files)
                    {
                        _logger?.Log($"Deleting: {item}");
                        _ioWrapper.DeleteFile(item);
                    }


                    _logger?.Log($"Creating new Shortcut in: '{startup}'{Environment.NewLine}Pointing to: '{exe}'");

                    _shell.CreateShortcut(_ioWrapper.GetCurrentDirectory(), startup, $"{EASY_SCRIPT_LAUNCHER}.exe");
                    await Task.Delay(200);
                }
                else
                    _logger?.Log($"Shortcut in:'{startup}'{Environment.NewLine}Pointing to: '{exe}'{Environment.NewLine}OK");
            }
            catch (Exception ex)
            {
                _logger?.Log($"Shortcut creation failure. Error; {ex.Message}");
                _messageBoxWrapper.Show(ex.Message, "Shortcut creation failure", MessageBoxButton.OK, MessageBoxImage.Error);
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

                _logger?.Log($"'{settings}' are OK");
            }
            catch (Exception ex)
            {
                _messageBoxWrapper.Show(ex.Message, settings, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }
        #endregion

        #region TASK MONITOR
        public bool SyncTaskMonitor(string scriptsLocation)
        {
            var sourceTaskMonitorPath = Path.Combine(_ioWrapper.GetCurrentDirectory(), TASK_MONITOR);
            _fileChecker.EnsureOnlyOneFileIsDeployed(scriptsLocation, $"{TASK_MONITOR}.ps1");

            var areMonitorsAvailable = _ioWrapper.GetFiles(sourceTaskMonitorPath, "*.ps1")
                .Where(x => x.Contains($"{TASK_MONITOR}", StringComparison.OrdinalIgnoreCase))
                .Any();

            if (!areMonitorsAvailable)
            {
                _logger?.Log($"FatalError: {TASK_MONITOR} could not be found. Rebuild or download the app again!");
                throw new Exception($"FatalError: {TASK_MONITOR} could not be found. Rebuild or download the app again!");
            }

            var syncResult = _fileChecker.SyncLatestFileVersion(sourceTaskMonitorPath, scriptsLocation, $"{TASK_MONITOR}.ps1");

            return syncResult;
        }
        #endregion
    }
}