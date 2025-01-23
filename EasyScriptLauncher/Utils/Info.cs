using System;

namespace EasyScriptLauncher.Utils
{
    internal class Info
    {
        private readonly Logger _logger;

        public Info(Logger logger)
        {
            _logger = logger;
        }
        internal void StartingScript(string path)
        {
            string text = $"Starting script: {path}{Environment.NewLine}";
            LogInLoggerAndConsole(text);
        }

        internal void ScriptLoadingFailed(string script, string message)
        {
            string text = $"Error occured while starting script: {script}{Environment.NewLine}Error: {message}{Environment.NewLine}";
            LogInLoggerAndConsole(text);
        }
        internal void ScriptLoadingFailed()
        {
            string text = $"Error occured while starting script!{Environment.NewLine}";
            LogInLoggerAndConsole(text);
        }

        internal void FillTheSettings(string path)
        {
            string text = $"Please state the valid path to ps scripts in settings.{Environment.NewLine}Settings location: {path}{Environment.NewLine}";
            LogInLoggerAndConsole(text);
        }

        internal void ScriptStartedSuccessfully(string script)
        {
            string text = $"Script {script} started successfully{Environment.NewLine}";
            LogInLoggerAndConsole(text);
        }

        internal void GenericError(string message)
        {
            string text = $"An Error occured {message}{Environment.NewLine}";
            LogInLoggerAndConsole(text);
        }

        internal void NoScriptsFound(string path, bool searchRecursively)
        {
            string tmp = searchRecursively ? "true" : "false";
            string text = $"No scripts found!{Environment.NewLine}Location: {path}{Environment.NewLine}Search Recursively: {tmp}{Environment.NewLine}";
            LogInLoggerAndConsole(text);
        }

        internal void Done()
        {
            string text = $"Completed!{Environment.NewLine}";
            LogInLoggerAndConsole(text);
        }

        internal void ScriptCompleted()
        {
            string text = $"Script execution finished successfully.{Environment.NewLine}";
            LogInLoggerAndConsole(text);
        }

        private void LogInLoggerAndConsole(string text)
        {
            _logger.Log(text);
            Console.WriteLine(text);
        }
    }
}
