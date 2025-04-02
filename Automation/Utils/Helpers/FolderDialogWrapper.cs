using Ookii.Dialogs.Wpf;
using System;

namespace Automation.Utils.Helpers
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

        internal string ShowFileDialog_ReturnPath()
        {
            var folderDialog = new VistaOpenFileDialog();

            var dialogResult = folderDialog.ShowDialog();

            if (dialogResult == true)
                return folderDialog.FileName;

            return string.Empty;
        }
    }
}