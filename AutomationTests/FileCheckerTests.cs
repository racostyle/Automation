using Automation.Logging;
using Automation.Utils.Helpers.Abstractions;
using Automation.Utils.Helpers.FileCheck;
using Moq;

namespace AutomationTests
{
    [TestFixture]
    public class FileCheckerTests
    {
        FileChecker _fileChecker;
        Mock<IFileSystemWrapper> _ioWrapper;
        Mock<IFileInfoFactory> _fileInfoFactory;
        Mock<ILogger> _loggerMock;

        Func<string, DateTime, IFileInfoWrapper> BuildFileInfoWrapperFunc;

        private string _pathToDeployedFiles = "C:\\Scripts\\Ble";
        private string _pathToBaseFiles = "D:\\Base\\Files";
        private string _fileName = "Megamind.ps1";

        [SetUp]
        public void SetUp()
        {
            _ioWrapper = new Mock<IFileSystemWrapper>();
            _fileInfoFactory = new Mock<IFileInfoFactory>();
            _loggerMock = new Mock<ILogger>();

            BuildFileInfoWrapperFunc = (path, dateTime) =>
            {
                var obj = new Mock<IFileInfoWrapper>();
                obj.Setup(x => x.LastWriteTime).Returns(dateTime);
                obj.Setup(x => x.Name).Returns(Path.GetFileName(path));
                obj.Setup(x => x.FullName).Returns(path);
                return obj.Object;
            };

            _loggerMock.Setup(x => x.Log(It.IsAny<string>()));

            _fileChecker = new FileChecker(_loggerMock.Object, _ioWrapper.Object, _fileInfoFactory.Object);
        }

        [Test]
        public void SyncLatestFileVersion_NoNeedForSync_ReturnsSuccess()
        {
            _ioWrapper.SetupSequence(x => x.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns([Path.Combine(_pathToBaseFiles, _fileName)])
                .Returns([Path.Combine(_pathToDeployedFiles, _fileName)]);

            var now = DateTime.Now;

            _fileInfoFactory.Setup(x => x.Build(It.IsAny<string>()))
                .Returns((string path) => BuildFileInfoWrapperFunc(path, now));

            _ioWrapper.Setup(x => x.FileExists(It.IsAny<string>()))
                .Returns(true);

            var result = _fileChecker.SyncLatestFileVersion(_pathToBaseFiles, _pathToDeployedFiles, _fileName);

            Assert.That(result, Is.True);
        }

        [Test]
        public void SyncLatestFileVersion_SyncNeeded_ReturnsSuccess()
        {
            var copied = new List<string>();

            _ioWrapper.SetupSequence(x => x.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns([Path.Combine(_pathToBaseFiles, _fileName)])
                .Returns([Path.Combine(_pathToDeployedFiles, _fileName)]);

            _fileInfoFactory.Setup(x => x.Build(It.IsAny<string>()))
                .Returns((string path) => BuildFileInfoWrapperFunc(path, DateTime.Now));

            _ioWrapper.Setup(x => x.FileExists(It.IsAny<string>()))
                .Returns(true);

            _ioWrapper.Setup(x => x.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                 .Callback(() => copied.Add("File Copied"));

            var result = _fileChecker.SyncLatestFileVersion(_pathToBaseFiles, _pathToDeployedFiles, _fileName);

            Assert.That(result, Is.True);
            Assert.That(copied, Is.Not.Empty);
        }

        [Test]
        public void SyncLatestFileVersion_SyncNeededThrowsException_ReturnsException()
        {
            var copied = new List<string>();

            _ioWrapper.SetupSequence(x => x.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns([Path.Combine(_pathToBaseFiles, _fileName)])
                .Returns([Path.Combine(_pathToDeployedFiles, _fileName)]);

            _fileInfoFactory.Setup(x => x.Build(It.IsAny<string>()))
                .Returns((string path) => BuildFileInfoWrapperFunc(path, DateTime.Now));

            _ioWrapper.Setup(x => x.FileExists(It.IsAny<string>()))
                .Returns(true);

            _ioWrapper.Setup(x => x.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                 .Throws(new Exception("Mock Exception"));

            try
            {
                var result = _fileChecker.SyncLatestFileVersion(_pathToBaseFiles, _pathToDeployedFiles, _fileName);
                Assert.Fail("Method should be throwing exception");
            }
            catch (Exception ex)
            {
                Assert.Pass();
            }
        }

        [Test]
        public void EnsureOnlyOneFileIsDeployed_CompareExistingFiles_ReturnsSuccess()
        {
            var dateOld = DateTime.Now.AddHours(-1);
            var dateNew = DateTime.Now;
            var expectedFileName = Path.Combine(_pathToDeployedFiles, _fileName);
            var oldFileName = Path.Combine(_pathToDeployedFiles, $"OLD_{_fileName}");

            _ioWrapper.Setup(x => x.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns([expectedFileName, oldFileName]);

            _fileInfoFactory.SetupSequence(x => x.Build(It.IsAny<string>()))
                 .Returns(() => BuildFileInfoWrapperFunc(expectedFileName, dateNew))
                 .Returns(() => BuildFileInfoWrapperFunc(oldFileName, dateOld));

            _ioWrapper.Setup(x => x.DeleteFile(It.IsAny<string>()))
                .Callback(() => { });

            var result = _fileChecker.EnsureOnlyOneFileIsDeployed(_pathToDeployedFiles, _fileName);

            Assert.That(result.FullName.Equals(expectedFileName), Is.True);
            Assert.That(result.LastWriteTime == dateNew, Is.True);
        }
    }
}
