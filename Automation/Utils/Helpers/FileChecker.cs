using Automation.Utils.Helpers.Abstractions;
using System;
using System.IO;
using System.Linq;

namespace Automation.Utils.Helpers
{
    public class FileChecker
    {
        private readonly IIOWrapper _ioWrapper;

        public FileChecker(IIOWrapper ioWrapper)
        {
            _ioWrapper = ioWrapper;
        }

        internal bool SyncLatestFileVersion(string baseLocation, string targetLocation, string fileNameWithExtension)
        {
            var fileName = Path.GetFileNameWithoutExtension(fileNameWithExtension);
            var extension = Path.GetExtension(fileNameWithExtension);

            var mostRecent = _ioWrapper.GetFiles(baseLocation, $"*{extension}")
                .Where(x => x.Contains($"{fileName}", StringComparison.OrdinalIgnoreCase))
                .Select(x => new FileInfo(x))
                .OrderByDescending(x => x.LastWriteTime)
                .FirstOrDefault();

            var deployedFile = EnsureOnlyOneFileIsDeployed(targetLocation, fileNameWithExtension);
            bool isUpToDate = CheckIfDeployedFileIsLatest(mostRecent, deployedFile);
            var deployedFileFullName = Path.Combine(targetLocation, mostRecent.Name);

            try
            {
                if (!isUpToDate || deployedFile.Length == 0)
                    _ioWrapper.CopyFile(mostRecent.FullName, deployedFileFullName, true);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return _ioWrapper.FileExists(deployedFileFullName);
        }

        private static bool CheckIfDeployedFileIsLatest(FileInfo mostRecent, FileInfo deployedFile)
        {
            if (deployedFile.LastWriteTime != mostRecent.LastWriteTime)
                return false;

            return true;
        }

        internal FileInfo EnsureOnlyOneFileIsDeployed(string targetLocation, string fileNameWithExtension)
        {
            var fileName = Path.GetFileNameWithoutExtension(fileNameWithExtension);
            var extension = Path.GetExtension(fileNameWithExtension);

            var deployedFiles = _ioWrapper.GetFiles(targetLocation, $"*{extension}")
              .Where(x => x.Contains($"{fileName}", StringComparison.OrdinalIgnoreCase))
              .Select(x => new FileInfo(x))
              .OrderByDescending(x => x.LastWriteTime)
              .ToArray();

            for (var i = 1; i < deployedFiles.Length; i++)
                _ioWrapper.DeleteFile(deployedFiles[i].FullName);

            return deployedFiles.FirstOrDefault();
        }
    }
}
