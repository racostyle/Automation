using ConfigLib;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Automation.Utils
{
    public class DeployHandler
    {
        private readonly SimpleShellExecutor _shell;
        private readonly StartupLocationsHandler _startupLocationsHandler;
        private readonly string EASY_SCRIPT_LAUNCHER = "EasyScriptLauncher";
        private readonly string TASK_MONITOR = "TaskMonitor";

        public DeployHandler(SimpleShellExecutor shell, StartupLocationsHandler startupLocationsHandler)
        {
            _shell = shell;
            _startupLocationsHandler = startupLocationsHandler;
        }

        #region EASY SCRIPT LAUNCHER
        public async Task<bool> CheckEasyScriptLauncher(string scriptsLocation, SettingsLoader settingsLoader, EnvironmentHandler environmentHandler)
        {
            var basePath = GetBaseEasyScriptLauncherPath(scriptsLocation);
            var pathToSettings = GetEasyScriptLauncherSettingsPath(scriptsLocation);

            ////Check Launcher
            //if (!Directory.Exists(basePath))
            //    Directory.CreateDirectory(basePath);

            //var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*", SearchOption.TopDirectoryOnly)
            //    .Where(x => Path.GetFileNameWithoutExtension(x)
            //        .Contains(EASY_SCRIPT_LAUNCHER, StringComparison.OrdinalIgnoreCase) || Path.GetFileNameWithoutExtension(x).Contains("ConfigLib", StringComparison.OrdinalIgnoreCase))
            //    .ToArray();

            //foreach (var file in files)
            //{
            //    var newFile = Path.Combine(basePath, Path.GetFileName(file));
            //    if (File.Exists(newFile))
            //    {
            //        var fbase = new FileInfo(file);
            //        var fnew = new FileInfo(newFile);
            //        if (fbase.LastWriteTime != fnew.LastWriteTime)
            //            File.Copy(file, newFile, true);
            //    }
            //    else
            //    {
            //        File.Copy(file, newFile);
            //    }
            //}

            //Check Settings
            if (!File.Exists(pathToSettings))
            {
                var settings = settingsLoader.LoadSettings(pathToSettings);
                settingsLoader.SaveSettings(pathToSettings, settings);
                await Task.Delay(200);
            }

            CheckScriptLauncherSettings(scriptsLocation);

            //Check for Shortcut
            var startupPath = _startupLocationsHandler.GetCommonStartupFolderPath();
            var doesShortcutExist = Directory.GetFiles(startupPath, "*").Any(x => x.Contains(EASY_SCRIPT_LAUNCHER, StringComparison.OrdinalIgnoreCase));

            return doesShortcutExist;
        }

        public async Task<bool> SetupEasyScriptLauncher(string scriptsLocation, EnvironmentHandler environmentHandler, SettingsLoader scriptLoader)
        {
            bool result = true;
            string startup = string.Empty;

            if (environmentHandler.IsMultiuser)
                startup = _startupLocationsHandler.GetCurrentUserStartupFolder();
            else
                startup = _startupLocationsHandler.GetCommonStartupFolderPath();

            if (!File.Exists(Path.Combine(startup, $"{EASY_SCRIPT_LAUNCHER}.lnk")))
                result = await CreateShortcut(scriptLoader, scriptsLocation, startup, environmentHandler);

            return result;
        }

        private async Task<bool> CreateShortcut(SettingsLoader scriptLoader, string scriptsLocation, string commonStartup, EnvironmentHandler environmentHandler)
        {
            if (!File.Exists(Path.Combine(commonStartup, $"{EASY_SCRIPT_LAUNCHER}.lnk")))
            {
                _shell.CreateShortcut(Directory.GetCurrentDirectory(), commonStartup, EASY_SCRIPT_LAUNCHER);
                await Task.Delay(1000);
            }

            var result = await CheckEasyScriptLauncher(scriptsLocation, scriptLoader, environmentHandler);
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

            var resultLauncher = await SetupEasyScriptLauncher(scriptsLocation, environmentHandler, scriptLoader);

            return resultLauncher;
        }

        public bool CheckScriptLauncherSettings(string scriptsLocation)
        {
            var pathToSettings = GetEasyScriptLauncherSettingsPath(scriptsLocation);
            if (!File.Exists(pathToSettings))
                return false;

            try
            {
                var text = File.ReadAllText(pathToSettings);
                var config = JsonSerializer.Deserialize<Config>(text);
                config.ScriptsFolder = scriptsLocation;

                text = JsonSerializer.Serialize(config);
                File.WriteAllText(pathToSettings, text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, pathToSettings);
                return false;
            }
            return true;
        }

        private string GetEasyScriptLauncherSettingsPath(string scriptsLocation)
        {
            var settingsName = $"{EASY_SCRIPT_LAUNCHER}_Settings.json";
            var basePath = GetBaseEasyScriptLauncherPath(scriptsLocation);
            return Path.Combine(basePath, settingsName);
        }

        private string GetBaseEasyScriptLauncherPath(string scriptsLocation)
        {
            //var root = Path.GetFullPath(Path.GetFullPath(Path.Combine(scriptsLocation, @"..")));
            var root = Directory.GetCurrentDirectory();
            return Path.Combine(root, EASY_SCRIPT_LAUNCHER);
        }
        #endregion

        #region TASK MONITOR
        public bool CheckTaskMonitor(string scriptsLocation, EnvironmentHandler environmentHandler)
        {
            var deployedMonitors = Directory.GetFiles(scriptsLocation, "*.ps1", SearchOption.AllDirectories)
                           .Where(x => x.Contains($"{TASK_MONITOR}", StringComparison.OrdinalIgnoreCase))
                           .ToArray();

            var result = deployedMonitors.Any();
            if (result)
                SetupOrUpdateTaskMonitor(scriptsLocation, environmentHandler);

            return result;
        }

        public bool SetupOrUpdateTaskMonitor(string scriptsLocation, EnvironmentHandler environmentHandler)
        {
            var baseMonitorPath = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.ps1", SearchOption.AllDirectories)
                .Where(x => x.Contains($"{TASK_MONITOR}", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (string.IsNullOrEmpty(baseMonitorPath))
                throw new Exception($"FatalError: {TASK_MONITOR} could not be found. Rebuild or download the app again!");

            var scriptsMonitorPath = DetermineNewFileName(baseMonitorPath, scriptsLocation, environmentHandler);

            if (!IsTaskMonitorUpdateRequired(baseMonitorPath, scriptsMonitorPath))
                return true;
            else
                DeleteAllMonitorsInScriptsLocation(scriptsLocation);

            try
            {
                File.Copy(baseMonitorPath, scriptsMonitorPath, true);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void DeleteAllMonitorsInScriptsLocation(string scriptsLocation)
        {
            var deployedMonitors = Directory.GetFiles(scriptsLocation, "*.ps1")
                            .Where(x => x.Contains($"{TASK_MONITOR}", StringComparison.OrdinalIgnoreCase))
                            .ToArray();

            foreach (var monitor in deployedMonitors)
                File.Delete(monitor);
        }

        private bool IsTaskMonitorUpdateRequired(string baseMonitorPath, string scriptsMonitorPath)
        {
            if (File.Exists(scriptsMonitorPath))
            {
                var baseMonitor = new FileInfo(baseMonitorPath);
                var newMonitor = new FileInfo(scriptsMonitorPath);

                if (baseMonitor.LastWriteTime != newMonitor.LastWriteTime)
                    return true;

                return false;
            }
            return true;
        }

        private string DetermineNewFileName(string filePath, string scriptsLocation, EnvironmentHandler environmentHandler)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);

            if (environmentHandler.IsSingleUser)
                return Path.Combine(scriptsLocation, filePath);
            else
                return Path.Combine(scriptsLocation, $"{fileName}_{environmentHandler.ProfileName}{extension}");
        }
        #endregion
    }
}