using Automation.Utils;
using Automation.Utils.Helpers.Abstractions;
using ConfigLib;
using Moq;

namespace AutomationTests
{
    public class DeployerTests
    {
        Deployer _deployer;
        Mock<ISimpleShellExecutor> _simpleShellExecutorMock;
        Mock<IEnvironmentInfo> _environmentInfoMock;
        Mock<IFileChecker> _fileCheckerMock;
        Mock<IFileSystemWrapper> _ioWrapperMock;
        Mock<ISettingsLoader> _settingsLoaderMock;

        [SetUp]
        public void Setup()
        {
            _simpleShellExecutorMock = new Mock<ISimpleShellExecutor>();
            _environmentInfoMock = new Mock<IEnvironmentInfo>();
            _fileCheckerMock = new Mock<IFileChecker>();
            _ioWrapperMock = new Mock<IFileSystemWrapper>();
            _settingsLoaderMock = new Mock<ISettingsLoader>();

            _deployer = new Deployer(
                _simpleShellExecutorMock.Object,
                _environmentInfoMock.Object,
                _fileCheckerMock.Object,
                _ioWrapperMock.Object,
                _settingsLoaderMock.Object);
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}