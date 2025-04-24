using Automation.Logging;
using Automation.Utils;
using Moq;

namespace AutomationTests
{
    [TestFixture]
    public class SimpleShellExecutorTest
    {
        Mock<ILogger> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger>();
        }

        [Test]
        public async Task Execute_RunScript_ReturnSuccess()
        {
            var executor = new SimpleShellExecutor(_loggerMock.Object);

            var expectedResult = "Ble Ble";
            var result = executor.Execute($"Write-Host {expectedResult}", Directory.GetCurrentDirectory(), false);

            Assert.That(result.Contains(expectedResult, StringComparison.OrdinalIgnoreCase), Is.True);
        }
    }
}
