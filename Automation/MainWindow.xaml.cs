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
        public MainWindow()
        {
            InitializeComponent();

            var handlers = new IVisualHandler[]
            {   
                new Handler_TextBox()
            };

            var handler = new VisualTreeAdapter(handlers);

            handler.PackVisualData(this);
        }


    }

    public class VisualTreeAdapter
    {
        private readonly IVisualHandler[] _visualHandlers;

        public VisualTreeAdapter(IVisualHandler[] visualHandlers)
        {
            _visualHandlers = visualHandlers;
        }

        public void PackVisualData(Visual visualTree)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visualTree); i++)
            {
                Visual childVisual = (Visual)VisualTreeHelper.GetChild(visualTree, i);

                // Process control
                ScrapeControlData(childVisual);

                // Recurse into children
                PackVisualData(childVisual);
            }
        }

        private void ScrapeControlData(Visual childVisual)
        {
            if (childVisual is TextBox)
            {

            }
        }
    }

    public class Handler_TextBox : IVisualHandler
    {
        public void AssignValueToVisual(Visual visual, string value)
        {
            if (DoesMatchTo(visual))
            {
                ((TextBox)visual).Text = value;
            } 
        }

        public bool DoesMatchTo(Visual visual)
        {
            return visual is TextBox;
        }

        public string GetVisualNameWithoutPrefix(Visual visual)
        {
            return DoesMatchTo(visual) ? new string(((TextBox)visual).Name.SkipWhile(x => char.IsLower(x)).ToArray()) : "";
        }

        public string GetVisualValue(Visual visual)
        {
            return DoesMatchTo(visual) ? ((TextBox)visual).Text : "";
        }
    }
}