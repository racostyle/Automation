using Automation.Logging;
using Automation.Utils.Helpers.Abstractions;
using System;
using System.IO;
using System.Linq;

namespace Automation.Utils.Helpers.FileCheck
{
    public class FileChecker : IFileChecker 
    {
        private readonly ILogger _logger;
        private readonly IFileSystemWrapper _ioWrapper;
        private readonly IFileInfoFactory _factory;

        public FileChecker(ILogger logger, IFileSystemWrapper ioWrapper, IFileInfoFactory factory)
        {
            _logger = logger;
            _ioWrapper = ioWrapper;
            _factory = factory;
        }

        public bool SyncLatestFileVersion(string baseLocation, string targetLocation, string fileNameWithExtension)
        {
            var fileName = Path.GetFileNameWithoutExtension(fileNameWithExtension);
            var extension = Path.GetExtension(fileNameWithExtension);

            var mostRecent = _ioWrapper.GetFiles(baseLocation, $"*{extension}")
                .Where(x => x.Contains($"{fileName}", StringComparison.OrdinalIgnoreCase))
                .Select(x => _factory.Build(x))
                .OrderByDescending(x => x.LastWriteTime)
                .FirstOrDefault();

            var deployedFile = EnsureOnlyOneFileIsDeployed(targetLocation, fileNameWithExtension);
            bool isUpToDate = CheckIfDeployedFileIsLatest(mostRecent, deployedFile);
            var destFileFullName = Path.Combine(targetLocation, mostRecent.Name);

            try
            {
                if (!isUpToDate || deployedFile == null)
                    _ioWrapper.CopyFile(mostRecent.FullName, destFileFullName, true);
                else
                    _logger?.Log($"Files are up to date. Files checked: '{mostRecent.Name}'");
            }
            catch (Exception ex)
            {
                _logger?.Log($"Error while syncing the files. Error: '{ex.Message}'");
                throw ex;
            }

            return _ioWrapper.FileExists(destFileFullName);
        }

        private bool CheckIfDeployedFileIsLatest(IFileInfoWrapper mostRecent, IFileInfoWrapper deployedFile)
        {
            if (deployedFile == null)
                return false;

            if (deployedFile.LastWriteTime != mostRecent.LastWriteTime)
                return false;

            return true;
        }

        public IFileInfoWrapper EnsureOnlyOneFileIsDeployed(string targetLocation, string fileNameWithExtension)
        {
            var fileName = Path.GetFileNameWithoutExtension(fileNameWithExtension);
            var extension = Path.GetExtension(fileNameWithExtension);

            var deployedFiles = _ioWrapper.GetFiles(targetLocation, $"*{extension}")
              .Where(x => x.Contains($"{fileName}", StringComparison.OrdinalIgnoreCase))
              .Select(x => _factory.Build(x))
              .OrderByDescending(x => x.LastWriteTime)
              .ToArray();

            for (var i = 1; i < deployedFiles.Length; i++)
                _ioWrapper.DeleteFile(deployedFiles[i].FullName);

            return deployedFiles.FirstOrDefault();
        }
    }
}
