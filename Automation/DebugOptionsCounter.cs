using System.Threading.Tasks;

namespace Automation
{
    internal class DebugOptionsCounter
    {
        private int _devOptionCounter = 0;
        private Task? _resetCounterTask;

        internal bool DoOpenWindow()
        {
            if (_devOptionCounter < 4)
            {
                _devOptionCounter++;
                if (_devOptionCounter == 1)
                {
                    _resetCounterTask = Task.Run(async () =>
                    {
                        await Task.Delay(2000);
                        _devOptionCounter = 0;
                    });
                }
            }
            else
            {
                _devOptionCounter = 0;
                return true;
            }
            return false;
        }

    }
}
