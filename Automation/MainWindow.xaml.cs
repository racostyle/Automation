using Automation.ConfigurationAdapter;
using Automation.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Text.Json;
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
        private readonly VisualTreeAdapter _visualTreeAdapter;
        private readonly Deployer _deployer;

        public MainWindow()
        {
            if (!IsRunningAsAdministrator())
            {
                MessageBox.Show("Start this app as admin!");
                Environment.Exit(0);
            }

            InitializeComponent();

            _configsWrapper = new ComboBoxWrapper_TaskMonitorConfigs(cbbConfigs);

            _visualTreeAdapter = new VisualTreeAdapterBuilder()
                .Add_HandlerTextBox()
                .Add_HandlerCheckBox()
                .ConfigureToUsePrefixes(false)
                .Build();

            _deployer = new Deployer(new SimpleShellExecutor());

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
            if (File.Exists("appsettings.json"))
            {
                var json = File.ReadAllText("appsettings.json");
                var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                _visualTreeAdapter.Unpack(this, config);

                var result = await _deployer.CheckEasyScriptLauncher(tbScriptsLocation.Text, new ConfigLib.SettingsLoader());
                ColorButton(result, btnSetupScripLauncher);

                result = _deployer.CheckTaskMonitor(tbScriptsLocation.Text);
                ColorButton(result, btnSetupTaskMonitor);


                HideOverlay();
            }

            if (!CheckScriptsLocation())
            {
                tbScriptsLocation.Text = "C:\\Delivery\\Automation\\Scripts";
                if (!Directory.Exists(tbScriptsLocation.Text))
                {
                    Directory.CreateDirectory(tbScriptsLocation.Text);
                    MessageBox.Show("Directory 'C:\\Delivery\\Automation\\Scripts' was created");
                }
                try
                {
                    _deployer.ChangeScriptLauncherSettings(tbScriptsLocation.Text);
                    SaveConfig();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("EasyScriptLauncher_Settings.json could not be saved!" + Environment.NewLine + ex.Message);
                }
            }

            LoadConfigs();

            HideOverlay();
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
            SaveConfig();
        }

        private void SaveConfig()
        {
            var config = _visualTreeAdapter.Pack(this);

            var text = JsonSerializer.Serialize(config);
            try
            {
                File.WriteAllText("appsettings.json", text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not create config! ERROR: " + ex.Message);
            }
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

                Window_TaskMonitor_Config secWindow = new Window_TaskMonitor_Config(_visualTreeAdapter, tbScriptsLocation.Text, configLocation);
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

                Window_TaskMonitor_Config secWindow = new Window_TaskMonitor_Config(_visualTreeAdapter, tbScriptsLocation.Text, string.Empty);
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
            var result = await _deployer.SetupEasyScriptLauncher(tbScriptsLocation.Text, new ConfigLib.SettingsLoader());
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
                SaveConfig();
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
                var window = new DeveloperOptionsWindow(_deployer, tbScriptsLocation.Text);
                window.ShowDialog();
            }
        }
        #endregion
    }
}
