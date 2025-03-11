using System.Threading;
using System.Threading.Tasks;

namespace Automation.Utils
{
    internal class DebugOptionsCounter
    {
        private int _devOptionCounter = 0;
        private Task? _resetCounterTask;
        private CancellationTokenSource? _tokenSource;

        internal bool DoOpenWindow()
        {
            if (_devOptionCounter < 4)
            {
                _devOptionCounter++;
                if (_devOptionCounter == 1)
                {
                    _resetCounterTask = Task.Run(async () =>
                    {
                        await Task.Delay(3000);
                        _devOptionCounter = 0;
                    }, SetupToken().Token);
                }
            }
            else
            {
                _devOptionCounter = 0;
                _tokenSource?.Cancel();
                return true;
            }
            return false;
        }

        private CancellationTokenSource SetupToken()
        {
            if (_tokenSource != null)
            {
                _tokenSource?.Cancel();
                _tokenSource?.Dispose();
            }
            _tokenSource = new CancellationTokenSource();
            return _tokenSource;
        }

    }
}
