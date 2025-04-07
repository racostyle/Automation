namespace Automation.Logging
{
    public interface ILogger
    {
        void Dispose();
        string GetLastLines();
        string GetLog();
        void Log(string message);
    }
}