using System.IO;

namespace Automation.Utils.Helpers.Abstractions
{
    public interface IFileChecker
    {
        FileInfo EnsureOnlyOneFileIsDeployed(string targetLocation, string fileNameWithExtension);
        bool SyncLatestFileVersion(string baseLocation, string targetLocation, string fileNameWithExtension);
    }
}