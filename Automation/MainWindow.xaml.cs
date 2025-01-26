using Automation.ConfigurationAdapter;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using static System.Net.Mime.MediaTypeNames;

namespace Automation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
        }

        #region CLOSED & LOADED HANDLERS
        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            var adapter = BuildHandler();

            var config = adapter.Pack(this);

            var text = JsonSerializer.Serialize(config);
            File.WriteAllText("appsettings.json", text);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Delay(100);

            var adapter = BuildHandler();

            if (!File.Exists("appsettings.json"))
                return;

            var json = File.ReadAllText("appsettings.json");
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            adapter.Unpack(this, config);

        }

        private VisualTreeAdapter BuildHandler()
        {
            var handlers = new IVisualHandler[]
            {
                 new Handler_TextBox(),
                 new Handler_CheckBox()
            };

            return new VisualTreeAdapter(handlers);
        }
        #endregion
    }
}