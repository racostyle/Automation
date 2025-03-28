using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace Automation
{
    /// <summary>
    /// Interaction logic for EnvironmentSettingsWindow.xaml
    /// </summary>
    public partial class EnvironmentSettingsWindow : Window
    {
        internal string EnvironmentType { get; private set; }

        public EnvironmentSettingsWindow()
        {
            InitializeComponent();
        }

        private void OnOptionClicked(object sender, RoutedEventArgs e)
        {
            var content = ((System.Windows.Controls.Button)sender).Content.ToString();

            if (string.IsNullOrEmpty(content))
                return;

            if (content.Contains("single", StringComparison.OrdinalIgnoreCase))
            {
                if (ShowConfirmation(content))
                {
                    EnvironmentType = content.Split(' ').FirstOrDefault();
                    this.DialogResult = true;
                }
            }
            else
            {
                if (ShowConfirmation(content))
                {
                    EnvironmentType = content.Split(' ').FirstOrDefault();
                    this.DialogResult = true;
                }
            }
        }

        private bool ShowConfirmation(string env)
        {
            DialogResult result1 = System.Windows.Forms.MessageBox.Show($"{env} option selected", "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

            if (result1 == System.Windows.Forms.DialogResult.OK)
            {
                DialogResult result2 = System.Windows.Forms.MessageBox.Show($"Are you sure you want {env} option?", "Safety Check", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result2 == System.Windows.Forms.DialogResult.Yes)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
