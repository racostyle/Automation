using System.Diagnostics;
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
                Verb = "RunAs",
            };

            Process process = Process.Start(startInfo);
            return process;
        }
        public Process Execute2(string command, string workingDirectory, bool visible = true, bool asAdmin = true)
        {
            var finalCommand = new StringBuilder();
            finalCommand.Append(command);
            finalCommand.Append(!visible ? "-WindowStyle Hidden" : string.Empty);
            finalCommand.Append(!asAdmin ? "-Verb RunAs" : string.Empty);


            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{finalCommand.ToString()}\"",
                UseShellExecute = visible,
                CreateNoWindow = !visible,
                WorkingDirectory = workingDirectory,
            };

            Process process = Process.Start(startInfo);
            return process;
        }
    }
}
