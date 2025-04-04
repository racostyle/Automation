namespace Automation.Utils.Helpers.Abstractions
{
    public interface IIOWrapper
    {
        void CopyFile(string sourceFileName, string destFileName, bool overwrite = false);
        void DeleteFile(string file);
        string[] GetFiles(string targetLocation, string extension = "");
        bool FileExists(string file);
    }
}