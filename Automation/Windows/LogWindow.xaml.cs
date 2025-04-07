using Automation.Logging;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Automation.Windows
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window
    {
        private readonly ILogger _logger;
        private bool isActive;

        public LogWindow(ILogger logger)
        {
            InitializeComponent();
            _logger = logger;
            isActive = true;
            _ = Task.Run(async () => await Update());

            this.Closed += OnClosed;
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            isActive = false;
        }

        private async Task Update()
        {
            while (isActive)
            {
                string log = _logger.GetLog();
                tbLog.Dispatcher.Invoke(() =>
                {
                    tbLog.Text = log;
                });
                await Task.Delay(1000);
            }
        }
    }
}
