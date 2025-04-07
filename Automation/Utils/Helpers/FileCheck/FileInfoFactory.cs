namespace Automation.Utils.Helpers.FileCheck
{
    public class FileInfoFactory : IFileInfoFactory
    {
        public IFileInfoWrapper Build(string path)
        {
            return new FileInfoWrapper(path);
        }
    }

    public interface IFileInfoFactory
    {
        IFileInfoWrapper Build(string path);
    }
}
