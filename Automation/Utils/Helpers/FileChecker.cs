using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace Automation.Utils.Helpers
{
    public class FileChecker
    {
        internal void CheckFileVersion(string baseLocation, string targetLocation, string fileNameWithoutEnding)
        {
            var mostRecent = Directory.GetFiles(baseLocation, "*.ps1")
                .Where(x => x.Contains($"{fileNameWithoutEnding}", StringComparison.OrdinalIgnoreCase))
                .Select(x => new FileInfo(x))
                .OrderByDescending(x => x.LastWriteTime)
                .FirstOrDefault();

            var deployedFiles = Directory.GetFiles(targetLocation, "*.ps1")
                .Where(x => x.Contains($"{fileNameWithoutEnding}", StringComparison.OrdinalIgnoreCase))
                .Select(x => new FileInfo(x))
                .OrderByDescending(x => x.LastWriteTime)
                .ToArray();

            var isUpToDate = true;

            foreach (var file in deployedFiles)
            {
                if (file.LastWriteTime != mostRecent.LastWriteTime)
                    isUpToDate = false;
            }

            if (!isUpToDate || deployedFiles.Length == 0)
            {
                var newFile = Path.Combine(targetLocation, mostRecent.Name);
                File.Copy(mostRecent.FullName, newFile, true);
            }
        }
    }
}
