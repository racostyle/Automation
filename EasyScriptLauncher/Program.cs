using EasyScriptLauncher.Utils;
using System;
using System.IO;

namespace EasyScriptLauncher
{
    internal class Program
    {
        static readonly string SETTINGS_FILE = "EasyScriptLauncher_Settings.json";

        static void Main(string[] args)
        {
            var info = new Info(new Logger());

            try
            {
                var config = new SettingsLoader().LoadSettings(SETTINGS_FILE);
                var processLauncher = new ProcessLauncher(info, config);

                if (!Directory.Exists(config.ScriptsFolder))
                {
                    info.FillTheSettings(Path.Combine(Directory.GetCurrentDirectory(), SETTINGS_FILE));
                    Environment.Exit(1);
                }

                string[] scripts;
                if (config.SearchForScriptsRecursively)
                    scripts = Directory.GetFiles(config.ScriptsFolder, "*.ps1", SearchOption.AllDirectories);
                else
                    scripts = Directory.GetFiles(config.ScriptsFolder, "*.ps1", SearchOption.TopDirectoryOnly);

                if (scripts.Length == 0)
                {
                    info.NoScriptsFound(config.ScriptsFolder, config.SearchForScriptsRecursively);
                    Environment.Exit(1);
                }

                foreach (var script in scripts)
                {
                    info.StartingScript(script);
                    processLauncher.StartProcess(script, config);
                }
            }
            catch (Exception ex)
            {
                info.GenericError(ex.Message);
                Environment.Exit(1);
            }
            info.Done();
        }
    }
}
