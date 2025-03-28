using System;
using System.IO;

namespace Automation
{
    public class StartupLocationsHandler
    {
        public string GetCommonStartupFolderPath()
        {
            var programData = Environment.GetEnvironmentVariable("ProgramData");
            var commonStartupPath = Path.Combine(programData, @"Microsoft\Windows\Start Menu\Programs\Startup");
            return commonStartupPath;
        }

        public string GetCurrentUserStartupFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        }
    }
}
