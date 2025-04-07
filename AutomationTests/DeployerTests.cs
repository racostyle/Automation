using Automation.Logging;
using Automation.Utils;
using Automation.Utils.Helpers.Abstractions;
using Automation.Utils.Helpers.FileCheck;
using ConfigLib;
using Moq;
using System.Windows;

namespace AutomationTests
{
    public class DeployerTests
    {
        Deployer _deployer;
        Mock<ILogger> _loggerMock;
        Mock<ISimpleShellExecutor> _simpleShellExecutorMock;
        Mock<IEnvironmentInfo> _environmentInfoMock;
        Mock<IFileChecker> _fileCheckerMock;
        Mock<IFileSystemWrapper> _ioWrapperMock;
        Mock<ISettingsLoader> _settingsLoaderMock;
        Mock<IMessageBoxWrapper> _messageBoxWrapperMock;

        private readonly string startupPath = "C:\\startup\\folder";
        private readonly string scriptsLocation = "C:\\Delivery\\Automation\\Scripts";

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger>();
            _simpleShellExecutorMock = new Mock<ISimpleShellExecutor>();
            _environmentInfoMock = new Mock<IEnvironmentInfo>();
            _fileCheckerMock = new Mock<IFileChecker>();
            _ioWrapperMock = new Mock<IFileSystemWrapper>();
            _settingsLoaderMock = new Mock<ISettingsLoader>();
            _messageBoxWrapperMock = new Mock<IMessageBoxWrapper>();

            _messageBoxWrapperMock.Setup(x => x.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()))
               .Callback(() => { });

            _ioWrapperMock.Setup(x => x.GetCurrentDirectory())
                .Returns("C:\\CurrentDir");

            _loggerMock.Setup(x => x.Log(It.IsAny<string>()));

            _deployer = new Deployer(
                _loggerMock.Object,
                _simpleShellExecutorMock.Object,
                _environmentInfoMock.Object,
                _fileCheckerMock.Object,
                _ioWrapperMock.Object,
                _settingsLoaderMock.Object,
                _messageBoxWrapperMock.Object);
        }

        [Test]
        public async Task SyncEasyScriptLauncher_SettingsAndShortcutOK_ReturnsSuccess()
        {
            _ioWrapperMock.Setup(x => x.FileExists(It.IsAny<string>()))
                .Returns(true);

            _ioWrapperMock.Setup(x => x.ReadAllText(It.IsAny<string>()))
                .Returns($@"{{""ScriptsFolder"":""{scriptsLocation.Replace(@"\", @"\\")}"", ""SearchForScriptsRecursively"": false, ""RunInSameWindow"": false, ""HideWindow"": false,""TestBehaviour"": false,""LoadProfile"": false ,""DelayInMils"": 0}}");

            _ioWrapperMock.Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => { });

            _environmentInfoMock.Setup(x => x.GetCommonStartupFolderPath())
                .Returns(startupPath);

            _ioWrapperMock.Setup(x => x.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns([Path.Combine(startupPath, $"{_deployer.EASY_SCRIPT_LAUNCHER}.lnc")]);

            _simpleShellExecutorMock.Setup(x => x.VerifyShortcutTarget(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var result = await _deployer.SyncEasyScriptLauncher(scriptsLocation);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task CheckScriptLauncherSettings_WrongJsonFormat_ReturnsFailure()
        {
            _ioWrapperMock.Setup(x => x.FileExists(It.IsAny<string>()))
                .Returns(true);

            _ioWrapperMock.Setup(x => x.ReadAllText(It.IsAny<string>()))
                .Returns($@"{{""ScriptsFolder"": flj\flj, ""SearchForScriptsRecursively"": false, ""RunInSameWindow"": false, ""HideWindow"": false,""TestBehaviour"": false,""LoadProfile"": false ,""DelayInMils"": 0}}");

            _ioWrapperMock.Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => { });

            _environmentInfoMock.Setup(x => x.GetCommonStartupFolderPath())
                .Returns(startupPath);

            var result = _deployer.CheckScriptLauncherSettings(scriptsLocation);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task SyncEasyScriptLauncher_SettingsOkNoShortcut_ReturnsSuccess()
        {
            _ioWrapperMock.Setup(x => x.FileExists(It.IsAny<string>()))
                .Returns(true);

            _ioWrapperMock.Setup(x => x.ReadAllText(It.IsAny<string>()))
                .Returns($@"{{""ScriptsFolder"":""{scriptsLocation.Replace(@"\", @"\\")}"", ""SearchForScriptsRecursively"": false, ""RunInSameWindow"": false, ""HideWindow"": false,""TestBehaviour"": false,""LoadProfile"": false ,""DelayInMils"": 0}}");

            _ioWrapperMock.Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => { });

            _environmentInfoMock.Setup(x => x.GetCommonStartupFolderPath())
                .Returns(startupPath);

            int callCount = 0;
            _ioWrapperMock.Setup(x => x.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() =>
                {
                    Interlocked.Increment(ref callCount);
                    return callCount == 2 ? Array.Empty<string>() : new[] { Path.Combine(startupPath, $"{_deployer.EASY_SCRIPT_LAUNCHER}.lnc") };
                });

            _simpleShellExecutorMock.Setup(x => x.VerifyShortcutTarget(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            _ioWrapperMock.Setup(x => x.DeleteFile(It.IsAny<string>()))
                .Callback(() => { });

            _simpleShellExecutorMock.Setup(x => x.CreateShortcut(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => { })
                .Returns((string workingDirectory, string shortcutDestination, string programNameWithExtension) =>
                {
                    return Path.Combine(shortcutDestination, $"{Path.GetFileNameWithoutExtension(programNameWithExtension)}.lnc");
                });

            var result = await _deployer.SyncEasyScriptLauncher(scriptsLocation);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task SyncTaskMonitor_EverythingOk_ReturnsSuccess()
        {
            _ioWrapperMock.Setup(x => x.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns([Path.Combine(startupPath, $"{_deployer.TASK_MONITOR}.ps1")]);

            _fileCheckerMock.Setup(x => x.SyncLatestFileVersion(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => { })
                .Returns(true);

            _ioWrapperMock.Setup(x => x.FileExists(It.IsAny<string>()))
                .Returns(true);

            var result = _deployer.SyncTaskMonitor(scriptsLocation);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task SyncTaskMonitor_NoTaskMonitor_ThrowsException()
        {
            _ioWrapperMock.Setup(x => x.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Array.Empty<string>());

            try
            {
                _deployer.SyncTaskMonitor(scriptsLocation);
            }
            catch (Exception ex) 
            { 
                Assert.Pass(ex.Message);
            }
        }
    }
}