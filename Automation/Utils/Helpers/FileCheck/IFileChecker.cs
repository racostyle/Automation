using System.IO;

namespace Automation.Utils.Helpers.FileCheck
{
    public interface IFileChecker
    {
        IFileInfoWrapper EnsureOnlyOneFileIsDeployed(string targetLocation, string fileNameWithExtension);
        bool SyncLatestFileVersion(string baseLocation, string targetLocation, string fileNameWithExtension);
    }
}