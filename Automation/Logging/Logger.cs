using System;
using System.IO;
using System.Text;

namespace Automation.Logging
{
    public class Logger : ILogger
    {
        private readonly StringBuilder _log = new StringBuilder();
        private string _lastMessage = string.Empty;

        public void Log(string message)
        {
            var time = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");
            _lastMessage = $"{time} {message}";
            _log.AppendLine($"{time} {message}");
        }

        public string GetLastLines()
        {
            return _lastMessage;
        }

        public void Dispose()
        {
            File.WriteAllText("Log.txt", _log.ToString());
        }

        public string GetLog()
        {
            return _log.ToString();
        }

    }
}
