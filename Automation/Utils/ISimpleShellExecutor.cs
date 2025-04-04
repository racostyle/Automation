using System.Diagnostics;

namespace Automation.Utils
{
    public interface ISimpleShellExecutor
    {
        void CreateShortcut(string workingDirectory, string shortcutDestination, string programNameWithExtension);
        string Execute(string command, string workingDirectory, bool visible = true, bool asAdmin = true, int timeout = 5000);
        Process ExecuteExe(string fileName);
        bool VerifyShortcutTarget(string target, string shortcutDestination, string programName);
    }
}