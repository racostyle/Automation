using System;

namespace Automation.Logging
{
    public interface ILogger : IDisposable
    {
        string GetLastLines();
        void Log(string message);
        string GetLog();
    }
}