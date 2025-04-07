using System;
using System.IO;

namespace Automation.Utils.Helpers.FileCheck
{
    public class FileInfoWrapper : IFileInfoWrapper
    {
        private FileInfo _info;

        public FileInfoWrapper(string path)
        {
            _info = new FileInfo(path);
        }

        public string FullName => _info.FullName;
        public string Name => _info.Name;
        public DateTime LastWriteTime => _info.LastWriteTime;
    }
}
