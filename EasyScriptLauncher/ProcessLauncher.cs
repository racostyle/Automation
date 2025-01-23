using EasyScriptLauncher.Utils;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace EasyScriptLauncher
{
    internal class ProcessLauncher
    {
        private readonly Info _info;
        private readonly Config _config;

        public ProcessLauncher(Info info, Config config)
        {
            _info = info;
            _config = config;
        }

        internal void StartProcess(string pathToScript, Config config)
        {
            bool useShellToExecute = !config.RunInSameWindow;
            string argument = BuildArguments(pathToScript, config);

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "powershell.exe",
                Arguments = argument,
                Verb = "RunAs",
                UseShellExecute = useShellToExecute,
                CreateNoWindow = config.HideWindow,
                RedirectStandardError = !useShellToExecute,
                RedirectStandardOutput = !useShellToExecute,
            };

            var process = new Process();
            process.StartInfo = startInfo;
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) =>
            {
                if (process.ExitCode == 0)
                    _info.ScriptCompleted();
                else _info.ScriptLoadingFailed();
            };

            process.Start();
           
            var task = Task.Run(async () =>
            {
                await Task.Delay(2000);
                if (!process.HasExited && process.ExitCode != 1)
                    _info.ScriptStartedSuccessfully(pathToScript);
            });

            Task.WaitAll(task);
        }

        internal string BuildArguments(string pathToScript, Config config)
        {
            var argumentBuilder = new StringBuilder();
            if (!config.LoadProfile)
                argumentBuilder.Append("-NoProfile ");

            argumentBuilder.Append("-ExecutionPolicy Bypass ");

            if (config.TestBehaviour)
                argumentBuilder.Append("-Command \"exit 0\"");
            else
                argumentBuilder.Append($"-File \"{pathToScript}\"");
            return argumentBuilder.ToString();
        }
    }
}