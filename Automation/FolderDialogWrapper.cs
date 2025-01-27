using Ookii.Dialogs.Wpf;

namespace Automation
{
    internal class FolderDialogWrapper
    {
        internal string ShowFolderDialog_ReturnPath()
        {
            var folderDialog = new VistaFolderBrowserDialog();
            folderDialog.RootFolder = Environment.SpecialFolder.MyComputer;

            var dialogResult = folderDialog.ShowDialog();

            if (dialogResult == true)
                return folderDialog.SelectedPath;

            return string.Empty;
        }
    }
}