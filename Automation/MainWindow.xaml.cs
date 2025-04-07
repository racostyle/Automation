using Automation.ConfigurationAdapter;
using Automation.Utils;
using System;
using System.Collections.Generic;
using Automation.Windows;
using System.IO;
using System.Security.Principal;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Automation.Utils.Helpers;
using ConfigLib;
using Automation.Utils.Helpers.FileCheck;
using Automation.Logging;

namespace Automation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DebugOptionsCounter _debugCounter;
        private readonly TaskMonitorConfigsComboBoxWrapper _configsWrapper;
        private readonly Deployer _deployer;
        private readonly SettingsHandler _settingsHandler;
        private readonly EnvironmentInfo _environmentInfo;
        private Window _debugWindow;
        private readonly Logger _logger;

        public MainWindow()
        {
            if (!IsRunningAsAdministrator())
            {
                MessageBox.Show("Start this app as admin!");
                Environment.Exit(0);
            }

            InitializeComponent();
            ShowOverlay();

            var visualTreeAdapter = new VisualTreeAdapterBuilder()
                .Add_HandlerTextBox()
                .Add_HandlerCheckBox()
                .ConfigureToUsePrefixes(false)
                .Build();

            _logger = new Logger();

            _settingsHandler = new SettingsHandler(visualTreeAdapter, _logger);
            _configsWrapper = new TaskMonitorConfigsComboBoxWrapper(cbbConfigs);
            _environmentInfo = new EnvironmentInfo();
            _deployer = new Deployer(
                new SimpleShellExecutor(),
                _environmentInfo,
                new FileChecker(new FileSystemWrapper(), new FileInfoFactory()),
                new FileSystemWrapper(),
                new SettingsLoader(),
                new MessageBoxWrapper());
            _debugCounter = new DebugOptionsCounter();
        }

        public static bool IsRunningAsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        #region CLOSED & LOADED HANDLERS
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var bootstrapper = new Bootstrapper(
                this,
                _settingsHandler,
                _deployer,
                tbScriptsLocation,
                btnSetupScripLauncher,
                btnSetupTaskMonitor,
                new SettingsLoader(),
                ColorButton);

            await bootstrapper.Init();

            LoadConfigs();
            HideOverlay();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _settingsHandler.Pack(this);
            _logger.Dispose();
            _debugWindow?.Close();
        }

        #endregion

        #region BUTTONS
        private void OnBtnEditAutomationScript_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!BaseScriptLocationSafetyCheck())
                    return;

                var configLocation = _configsWrapper.GetValue();
                if (string.IsNullOrEmpty(configLocation))
                    return;

                Window_TaskMonitor_Config secWindow = new Window_TaskMonitor_Config(_settingsHandler.VisualTreeAdapter, tbScriptsLocation.Text, configLocation);
                secWindow.ShowDialog();

                if (secWindow.DialogResult == true)
                    _logger.Log($"Config '{secWindow.FileName}' edited successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OnBtnNewAutomationScript_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!BaseScriptLocationSafetyCheck())
                    return;

                Window_TaskMonitor_Config secWindow = new Window_TaskMonitor_Config(_settingsHandler.VisualTreeAdapter, tbScriptsLocation.Text, string.Empty);
                secWindow.ShowDialog();

                if (secWindow.DialogResult == true) 
                {
                    _configsWrapper.Load(tbScriptsLocation.Text);
                    _logger.Log($"Config '{secWindow.FileName}' created");
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                _logger.Log($"Config creation failed. Error {ex.Message}");
            }
        }

        private void OnBtnDeleteAutomationScript_Click(object sender, RoutedEventArgs e)
        {
            var config = cbbConfigs.Text;
            if (!string.IsNullOrEmpty(config))
            {
                if (File.Exists(config))
                {
                    var dialog = MessageBox.Show($"Delete '{Path.GetFileName(config)}'?", "Delete config", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (dialog == MessageBoxResult.Yes)
                    {
                        File.Delete(config);
                        cbbConfigs.Items.Remove(config);
                        cbbConfigs.SelectedIndex = 0;
                        _logger.Log($"Config '{config}' deleted");
                    }
                }
            }
        }

        private async void OnBtnSetupScripLauncher_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlay();
            var result = await _deployer.SyncEasyScriptLauncher(tbScriptsLocation.Text);
            ColorButton(result, btnSetupScripLauncher);
            HideOverlay();
        }

        private void OnBtnSetupTaskMonitor_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlay();
            if (!BaseScriptLocationSafetyCheck())
                return;

            var result = _deployer.SyncTaskMonitor(tbScriptsLocation.Text);
            ColorButton(result, btnSetupTaskMonitor);
            HideOverlay();
        }

        private void OnBtnLoadScripts_Click(object sender, RoutedEventArgs e)
        {
            LoadConfigs();
        }

        private void OnTbScriptsLocation_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var dialogResult = new FolderDialogWrapper().ShowFolderDialog_ReturnPath();

            if (Directory.Exists(dialogResult))
            {
                tbScriptsLocation.Text = dialogResult;
                _deployer.CheckScriptLauncherSettings(tbScriptsLocation.Text);
                _settingsHandler.Pack(this);
                MessageBox.Show($"Scripts location changed to: {Environment.NewLine}'{dialogResult}");
            }
            else
                MessageBox.Show($"Directory '{dialogResult}' is not vaild!");
        }

        private void OnBtnStartEasyScriptLauncher_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var executor = new SimpleShellExecutor();
                executor.ExecuteExe(Path.Combine(Directory.GetCurrentDirectory(), "EasyScriptLauncher.exe"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not start EasyScriptLauncher. ERROR: " + ex.Message);
            }
        }

        private void OnBtnShowLocations_Click(object sender, RoutedEventArgs e)
        {
            if (_debugWindow == null)
            {
                _debugWindow = new DebugWindow(this, tbScriptsLocation.Text, _environmentInfo);
                _debugWindow.Closed += DebugWindow_Closed!;
                _debugWindow.Show();
            }
        }
        private void DebugWindow_Closed(object sender, EventArgs e)
        {
            _debugWindow.Closed -= DebugWindow_Closed!;
            _debugWindow = null;
        }

        #endregion

        #region AUXILIARY
        private bool BaseScriptLocationSafetyCheck()
        {
            if (!Directory.Exists(tbScriptsLocation.Text))
            {
                MessageBox.Show("Base script location not set or does not exist!");
                return false;
            }
            return true;
        }

        private void LoadConfigs()
        {
            if (!BaseScriptLocationSafetyCheck())
                return;

            _configsWrapper.Load(tbScriptsLocation.Text);

            ColorButton(cbbConfigs.Items.Count > 0, btnLoadScripts);
        }

        private void ColorButton(bool result, Button button)
        {
            if (result)
                button.Background = Brushes.DarkGreen;
            else
                button.Background = Brushes.DarkRed;
        }

        private void ShowOverlay()
        {
            recOverlay.Visibility = Visibility.Visible;
        }

        private void HideOverlay()
        {
            recOverlay.Visibility = Visibility.Hidden;
        }
        #endregion

        #region DEV OPTIONS
        private void Label_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_debugCounter.DoOpenWindow())
            {
                var window = new DeveloperOptionsWindow(_deployer, _environmentInfo, tbScriptsLocation.Text);
                window.ShowDialog();
            }
        }
        #endregion
    }
}
