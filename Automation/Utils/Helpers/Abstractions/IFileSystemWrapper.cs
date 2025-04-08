namespace Automation.Utils.Helpers.Abstractions
{
    public interface IFileSystemWrapper
    {
        void CopyFile(string sourceFileName, string destFileName, bool overwrite = false);
        void DeleteFile(string? file);
        string[] GetFiles(string targetLocation, string extension = "", string fileName = "");
        bool FileExists(string file);
        string GetCurrentDirectory();
        void WriteAllText(string path, string contents);
        string ReadAllText(string path);
    }
}