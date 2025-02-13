﻿using Automation.ConfigurationAdapter;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Automation.Windows
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        private readonly Window _parent;
        private readonly string _scriptsLocation;
        private readonly string _commonStartupLocation;

        public DebugWindow(Window parent, string scriptsLocation, string commonStartupLocation)
        {
            InitializeComponent();
            _parent = parent;
            _scriptsLocation = scriptsLocation;
            _commonStartupLocation = commonStartupLocation;
            _parent.LocationChanged += OnParent_LocationChanged;
            SetPosition();
        }

        private void OnParent_LocationChanged(object? sender, EventArgs e)
        {
            SetPosition();
        }

        private void SetPosition()
        {
            this.Left = _parent.Left + _parent.Width;
            this.Top = _parent.Top;
        }

        private void tbOpenScripts_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(_scriptsLocation))
                Process.Start("explorer.exe", _scriptsLocation);
        }

        private void tbOpenStartup_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(_scriptsLocation))
                Process.Start("explorer.exe", _commonStartupLocation);
        }

    }
}
