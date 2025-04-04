﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Automation.Utils
{
    public class SimpleShellExecutor
    {
        public Process ExecuteExe(string fileName)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = false,
                CreateNoWindow = true,
                Verb = "RunAs",
            };

            Process process = Process.Start(startInfo);
            return process;
        }

        public string Execute(string command, string workingDirectory, bool visible = true, bool asAdmin = true, int timeout = 5000)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
                UseShellExecute = visible,
                CreateNoWindow = !visible,
                RedirectStandardOutput = !visible,
                WorkingDirectory = workingDirectory,
                Verb = asAdmin ? "RunAs" : string.Empty
            };

            StringBuilder output = new StringBuilder();
            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        output.AppendLine(args.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit(timeout); 

                return output.ToString();
            }
        }

        public void CreateShortcut(string workingDirectory, string shortcutDestination, string programNameWithExtension)
        {
            var command = BuildCreateShortcutScript(workingDirectory, shortcutDestination, programNameWithExtension);
            Execute(command, Directory.GetCurrentDirectory(), false, true);
        }

        public bool VerifyShortcutTarget(string target, string shortcutDestination, string programName)
        {
            var command = BuildVerifyShortcutScript(target, shortcutDestination, programName);
            var result = Execute(command, Directory.GetCurrentDirectory(), false, true);
            return !result.Contains("INVALID", StringComparison.OrdinalIgnoreCase);
        }

        #region Scripts
        private string BuildCreateShortcutScript(string target, string shortcutDestination, string programNameWithExtension)
        {
            var programName = Path.GetFileNameWithoutExtension(programNameWithExtension);
            var extension = Path.GetExtension(programNameWithExtension);

            var shortcutPath = Path.Combine(shortcutDestination, $"{programName}.lnk");
            var targetPath = Path.Combine(target, $"{programName}{extension}");

            return @$"
                $shortcutPath = '{shortcutPath}'
                $targetPath = '{targetPath}' 
                $WScriptShell = New-Object -ComObject WScript.Shell
                $Shortcut = $WScriptShell.CreateShortcut($shortcutPath)
                
                $Shortcut.TargetPath = $targetPath
                $Shortcut.WorkingDirectory = [System.IO.Path]::GetDirectoryName($targetPath)
                $Shortcut.Save()";
        }

        private string BuildVerifyShortcutScript(string target, string shortcutDestination, string programNameWithExtension)
        {
            var programName = Path.GetFileNameWithoutExtension(programNameWithExtension);
            var extension = Path.GetExtension(programNameWithExtension);

            var shortcutPath = Path.Combine(shortcutDestination, $"{programName}.lnk");
            var targetPath = Path.Combine(target, $"{programName}{extension}");

            return $@"
                $shortcutPath = '{shortcutPath}'
                $targetPath = '{targetPath}'

                $WshShell = New-Object -ComObject WScript.Shell
                $Shortcut = $WshShell.CreateShortcut($shortcutPath)

                if ($Shortcut.TargetPath -eq $targetPath) {{
                    Write-Host OK
                }} else {{
                    Write-Host INVALID
                }}
            ";
        }
        #endregion
    }
}
