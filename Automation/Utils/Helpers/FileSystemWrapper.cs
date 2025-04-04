using System.IO;
using Automation.Utils.Helpers.Abstractions;

namespace Automation.Utils.Helpers
{
    public class FileSystemWrapper : IFileSystemWrapper
    {
        public string[] GetFiles(string targetLocation, string extension = "")
        {
            return Directory.GetFiles(targetLocation, $"*{extension}");
        }

        public void DeleteFile(string file)
        {
            File.Delete(file);
        }

        public void CopyFile(string sourceFileName, string destFileName, bool overwrite = false)
        {
            File.Copy(sourceFileName, destFileName, true);
        }

        public bool FileExists(string file)
        {
            return File.Exists(file);
        }

        public string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        public void WriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }
    }
}
