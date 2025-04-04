using System.Windows;
using Automation.Utils.Helpers.Abstractions;

namespace Automation.Utils.Helpers
{
    public class MessageBoxWrapper : IMessageBoxWrapper
    {
        public void Show(string message, string caption, MessageBoxButton buttonType, MessageBoxImage imageType)
        {
            MessageBox.Show(message, caption, buttonType, imageType);
        }
    }
}
