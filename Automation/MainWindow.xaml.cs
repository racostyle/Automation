﻿using Automation.Utils;
using Automation.Windows;
using System;
using System.IO;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Automation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DebugOptionsCounter _debugCounter;
        private readonly ComboBoxWrapper_TaskMonitorConfigs _configsWrapper;
        private readonly Deployer _deployer;
        private readonly SettingsHandler _settingsHandler;
        private Window _debugWindow;
        private EnvironmentHandler _environmentHandler;

        public MainWindow()
        {
            if (!IsRunningAsAdministrator())
            {
                MessageBox.Show("Start this app as admin!");
                Environment.Exit(0);
            }

            InitializeComponent();

            _configsWrapper = new ComboBoxWrapper_TaskMonitorConfigs(cbbConfigs);
            _deployer = new Deployer(new SimpleShellExecutor(), new StartupLocationsHandler());
            _debugCounter = new DebugOptionsCounter();
            _settingsHandler = new SettingsHandler();
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
            if (File.Exists(_settingsHandler.SETTINGS))
            {
                _settingsHandler.Unpack(this);

                var result = await _deployer.CheckEasyScriptLauncher(tbScriptsLocation.Text, new ConfigLib.SettingsLoader());
                ColorButton(result, btnSetupScripLauncher);

                result = _deployer.CheckTaskMonitor(tbScriptsLocation.Text);
                ColorButton(result, btnSetupTaskMonitor);

                HideOverlay();
            }
            else
            {
                SetupEnvironment();
                _settingsHandler.Pack(this);
            }

            if (!CheckScriptsLocation())
                CreateScriptsLocations();

            _environmentHandler = new EnvironmentHandler(tbEnvironmentType.Text);

            LoadConfigs();
            HideOverlay();
        }

        private void CreateScriptsLocations()
        {
            if (!Directory.Exists(tbScriptsLocation.Text))
            {
                Directory.CreateDirectory(tbScriptsLocation.Text);
                MessageBox.Show($"Directory '{tbScriptsLocation.Text}' was created");
            }

            var recurringPath = Path.Combine(tbScriptsLocation.Text, _settingsHandler.RECURRING_SCRIPTS_LOCATION);
            if (!Directory.Exists(recurringPath))
            {
                Directory.CreateDirectory(recurringPath);
                MessageBox.Show($"Directory '{recurringPath}' was created");
            }

            try
            {
                _deployer.ChangeScriptLauncherSettings(tbScriptsLocation.Text);
                if (!_settingsHandler.Pack(this))
                    MessageBox.Show("Could not create config!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("EasyScriptLauncher_Settings.json could not be saved!" + Environment.NewLine + ex.Message);
            }
        }

        private bool CheckScriptsLocation()
        {
            bool result = true;
            if (string.IsNullOrEmpty(tbScriptsLocation.Text))
                result = false;
            if (!Directory.Exists(tbScriptsLocation.Text))
                result = false;
            return result;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _settingsHandler.Pack(this);
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
                CenterChildOnInParent(secWindow);
                secWindow.ShowDialog();
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
                CenterChildOnInParent(secWindow);
                secWindow.ShowDialog();

                _configsWrapper.Load(tbScriptsLocation.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnBtnSetupScripLauncher_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlay();
            var result = await _deployer.SetupEasyScriptLauncher(tbScriptsLocation.Text, _environmentHandler, new ConfigLib.SettingsLoader());
            ColorButton(result, btnSetupScripLauncher);
            HideOverlay();
        }

        private void OnBtnSetupTaskMonitor_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlay();
            if (!BaseScriptLocationSafetyCheck())
                return;

            var result = _deployer.SetupTaskMonitor(tbScriptsLocation.Text);
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
                _deployer.ChangeScriptLauncherSettings(tbScriptsLocation.Text);
                if (!_settingsHandler.Pack(this))
                    MessageBox.Show("Could not create config!");
                else
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
                _debugWindow = new DebugWindow(this, tbScriptsLocation.Text, new StartupLocationsHandler(), _environmentHandler);
                _debugWindow.Closed += DebugWindow_Closed!;
                _debugWindow.Show();
            }
        }
        private void DebugWindow_Closed(object sender, EventArgs e)
        {
            _debugWindow.Closed -= DebugWindow_Closed!;
            _debugWindow = null;
        }

        private void OnEnvironmentSet(object sender, RoutedEventArgs e)
        {
            SetupEnvironment();
            CreateScriptsLocations();
        }

        private void SetupEnvironment()
        {
            var window = new EnvironmentSettingsWindow();
            CenterChildOnInParent(window);
            bool? dialog = window.ShowDialog();

            if (dialog == true)
            {
                SetupDefaultValues(window.EnvironmentType.Trim());
            }
            else
            {
                SetupDefaultValues("MULTI");
            }
            _environmentHandler = new EnvironmentHandler(tbEnvironmentType.Text);
            LoadConfigs();
        }

        private void SetupDefaultValues(string environmentType)
        {
            var content = environmentType;
            tbEnvironmentType.Text = content;

            if (content.Equals("SINGLE", StringComparison.OrdinalIgnoreCase))
                tbScriptsLocation.Text = _settingsHandler.GetDefaultScriptsLocation();
            else
            {
                tbScriptsLocation.Text = _settingsHandler.GetCurrentUserScriptLocation();
            }
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

        private void CenterChildOnInParent(Window child)
        {
            child.Owner = this;
            child.WindowStartupLocation = WindowStartupLocation.Manual;

            child.Left = this.Left + (this.Width - child.Width) / 2;
            child.Top = this.Top + (this.Height - child.Height) / 2;
        }
        #endregion

        #region DEV OPTIONS
        private void Label_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_debugCounter.DoOpenWindow())
            {
                var window = new DeveloperOptionsWindow(_deployer, tbScriptsLocation.Text, _environmentHandler, new StartupLocationsHandler());
                CenterChildOnInParent(window);
                window.ShowDialog();
            }
        }
        #endregion
    }
}
