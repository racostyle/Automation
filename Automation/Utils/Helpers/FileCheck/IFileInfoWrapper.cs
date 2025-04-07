using System;

namespace Automation.Utils.Helpers.FileCheck
{
    public interface IFileInfoWrapper
    {
        string FullName { get; }
        DateTime LastWriteTime { get; }
        string Name { get; }
    }
}