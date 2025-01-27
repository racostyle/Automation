using ConfigLib;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace Automation.Utils
{
    public class Deployer
    {
        private readonly SimpleShellExecutor _shell;

        public Deployer(SimpleShellExecutor shell)
        {
            _shell = shell;
        }

        public async Task<bool> CheckEasyScriptLauncher(string scriptsLocation)
        {
            //Check Settings
            var name = "EasyScriptLauncher";
            var settings = $"{name}_Settings.json";

            if (!File.Exists(settings))
            {
                var process = _shell.ExecuteExe(Path.Combine(Directory.GetCurrentDirectory(), $"{name}.exe"));
                await Task.Delay(2000);
                process?.Kill();
            }

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

            //Check for Shortcut
            var startupPath = GetCommonStartupFolderPathManual();
            var doesShortcutExist = Directory.GetFiles(startupPath, "*").Any(x => x.Contains(name, StringComparison.OrdinalIgnoreCase));

            return doesShortcutExist;
        }

        public static string GetCommonStartupFolderPathManual()
        {
            var programData = Environment.GetEnvironmentVariable("ProgramData");
            var commonStartupPath = Path.Combine(programData, @"Microsoft\Windows\Start Menu\Programs\Startup");
            return commonStartupPath;
        }

        public void SetupEasyScriptLauncher(string location)
        {

        }

        public bool CheckTaskMonitor(string scriptsLocation)
        {
            var programName = "TaskMonitor";
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), programName);
            var file = Path.GetFileName(Directory.GetFiles(basePath, "*.ps1").Where(x => x.Contains($"{programName}", StringComparison.OrdinalIgnoreCase)).FirstOrDefault());

            if (string.IsNullOrEmpty(file))
                throw new Exception($"FatalError: {programName} could not be found. Rebuild or download the app again!");

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
                File.Copy(Path.Combine(Path.Combine(basePath, file)), path);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}