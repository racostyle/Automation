﻿using ConfigLib;
using EasyScriptLauncher.Utils;
using System;
using System.IO;
using System.Threading;

namespace EasyScriptLauncher
{
    internal class Program
    {
        static readonly string SETTINGS_FILE = "EasyScriptLauncher_Settings.json";

        static void Main(string[] args)
        {
            var info = new Info(new Logger());
            var config = new SettingsLoader().LoadSettings(Path.Combine(Directory.GetCurrentDirectory(), SETTINGS_FILE));

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

            if (config.DelayInMils > 0)
            {
                Console.WriteLine("Waiting for environment to set up!");
                Console.WriteLine($"Sleeping for {config.DelayInMils / 1000} seconds...");
                Thread.Sleep(config.DelayInMils);
            }

            var processLauncher = new ProcessLauncher(info, config);

            foreach (var script in scripts)
            {
                try
                {
                    info.StartingScript(script);
                    processLauncher.StartProcess(script, config);
                }
                catch (Exception ex)
                {
                    info.GenericError(ex.Message);
                }
            }

            info.Done();
        }
    }
}
