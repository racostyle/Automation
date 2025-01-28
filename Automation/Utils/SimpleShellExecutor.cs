using System.Diagnostics;
using System.IO;

namespace Automation.Utils
{
    public class SimpleShellExecutor
    {
        public Process ExecuteExe(string fileName)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Verb = "RunAs",
            };

            Process process = Process.Start(startInfo);
            return process;
        }

        public void CreateShortcut(string target, string shortcutDestination, string programName)
        {
            var command = BuildShortcutScript(target, shortcutDestination, programName);
            Execute(command, Directory.GetCurrentDirectory(), false, true);
        }

        public Process Execute(string command, string workingDirectory, bool visible = true, bool asAdmin = true)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
                UseShellExecute = visible,
                CreateNoWindow = !visible,
                WorkingDirectory = workingDirectory,
                Verb = asAdmin ? "RunAs" : string.Empty
            };

            Process process = Process.Start(startInfo);
            return process;
        }

        private string BuildShortcutScript(string target, string shortcutDestination, string programName)
        {
            var shortcutPath = Path.Combine(shortcutDestination, $"{programName}.lnk");
            var targetPath = Path.Combine(target, $"{programName}.exe");

            // Escaping quotes for use in PowerShell
            shortcutPath = shortcutPath.Replace(@"\", @"\\");  // Ensure the backslashes are escaped in PowerShell string
            targetPath = targetPath.Replace(@"\", @"\\");

            //enclose path in double quotes
            return @$"
                $shortcutPath = \""{shortcutPath}\""  
                $targetPath = \""{targetPath}\""     
                $WScriptShell = New-Object -ComObject WScript.Shell
                $Shortcut = $WScriptShell.CreateShortcut($shortcutPath)
                
                $Shortcut.TargetPath = $targetPath
                $Shortcut.WorkingDirectory = [System.IO.Path]::GetDirectoryName($targetPath)
                $Shortcut.Save()";
        }
    }
}
