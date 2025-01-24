using Automation.ConfigurationAdapter;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
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
            this.Loaded += MainWindow_Loaded; ;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Delay(100);

            var handlers = new IVisualHandler[]
            {
                 new Handler_TextBox()
            };

            var handler = new VisualTreeAdapter(handlers);

            //var config = handler.Pack(this);

            //var text = JsonSerializer.Serialize(config);
            //File.WriteAllText("appsettings.json", text);

            var json = File.ReadAllText("appsettings.json");
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            handler.Unpack(this, config);

        }
    }

    public class VisualTreeAdapter
    {
        private readonly IVisualHandler[] _visualHandlers;

        public VisualTreeAdapter(IVisualHandler[] visualHandlers)
        {
            _visualHandlers = visualHandlers;
        }

        public Dictionary<string, string> Pack(Visual visualTree)
        {
            var visuals = FetchAllVisuals(visualTree);

            var config = new Dictionary<string, string>();

            foreach (var visual in visuals)
            {
                foreach (var handler in _visualHandlers)
                {
                    if (handler.DoesMatchTo(visual))
                        config.Add(handler.GetVisualNameWithoutPrefix(visual), handler.GetVisualValue(visual));
                }
            }

            return config;
        }

        public Dictionary<string, string> Unpack(Visual visualTree, Dictionary<string,string> config)
        {
            if (config == null)
                return new Dictionary<string, string>();

            var recognized = GetAllRecognizedVisuals(visualTree);

            foreach (var visual in recognized)
            {
                foreach (var handler in _visualHandlers)
                {
                    var name = handler.GetVisualNameWithoutPrefix(visual);
                    if (handler.DoesMatchTo(visual) && config.ContainsKey(name))
                    {
                        handler.AssignValueToVisual(visual, config[name]);
                    }
                }
            }

            return config;
        }

        private List<Visual> GetAllRecognizedVisuals(Visual visualTree)
        {
            var visuals = FetchAllVisuals(visualTree);
            var recognized = new List<Visual>();

            foreach (var visual in visuals)
            {
                if (_visualHandlers.Any(x => x.DoesMatchTo(visual)))
                    recognized.Add(visual);
            }
            return recognized;
        }

        public List<Visual> FetchAllVisuals(Visual visualTree)
        {
            var visuals = new List<Visual>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visualTree); i++)
            {
                Visual childVisual = (Visual)VisualTreeHelper.GetChild(visualTree, i);
                visuals.Add(childVisual);
                // Recurse into children
                visuals.AddRange(FetchAllVisuals(childVisual));
            }
            return visuals;
        }

    }
}