using System;
using System.IO;

namespace Automation.Utils.Helpers
{
    public class EnvironmentInfo : IEnvironmentInfo
    {
        public string GetCommonStartupFolderPath()
        {
            var programData = Environment.GetEnvironmentVariable("ProgramData");
            var commonStartupPath = Path.Combine(programData, @"Microsoft\Windows\Start Menu\Programs\Startup");
            return commonStartupPath;
        }
    }
}
