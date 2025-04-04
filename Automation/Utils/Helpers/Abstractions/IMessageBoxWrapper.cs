using System.Windows;

namespace Automation.Utils.Helpers.Abstractions
{
    public interface IMessageBoxWrapper
    {
        void Show(string message, string caption, MessageBoxButton buttonType, MessageBoxImage imageType);
    }
}