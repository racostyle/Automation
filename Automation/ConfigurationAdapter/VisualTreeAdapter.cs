using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Automation.ConfigurationAdapter
{
    /// <summary>
    /// naming convention prefixes: tb - textbox, tbl - TextBlock, chb - CheckBox, cbb - ComboBox, rbtn - RadioButton
    /// Example: tbTextBox will come out from GetControlNameWithoutPrefix as TextBox
    /// </summary>
    /// 

    public class VisualTreeAdapter
    {
        private readonly string[] PREFIXES = new string[] { "tb", "tbl", "chb", "cbb", "rbtn" };

        public bool SavedByAdapter { get; private set; } = false;

        private readonly bool _usePrefixes;
        private readonly IVisualHandler[] _visualHandlers;

        public VisualTreeAdapter(IVisualHandler[] visualHandlers, bool usePrefixes = false)
        {
            _visualHandlers = visualHandlers;
            _usePrefixes = usePrefixes;
        }

        public Dictionary<string, string> Pack(Visual visualTree)
        {
            var recognized = GetAllRecognizedVisuals(visualTree);

            var config = new Dictionary<string, string>();

            foreach (var visual in recognized)
            {
                foreach (var handler in _visualHandlers)
                {
                    if (handler.DoesMatchTo(visual))
                        config.Add(handler.GetVisualNameWithoutPrefix(visual), handler.GetVisualValue(visual));
                }
            }

            return config;
        }

        public Dictionary<string, string> Unpack(Visual visualTree, Dictionary<string, string> config)
        {
            if (config == null)
                return new Dictionary<string, string>();

            SavedByAdapter = true;

            var recognized = GetAllRecognizedVisuals(visualTree);

            foreach (var visual in recognized)
            {
                foreach (var handler in _visualHandlers)
                {
                    var name = handler.GetVisualNameWithoutPrefix(visual);
                    if (handler.DoesMatchTo(visual) && config.ContainsKey(name))
                    {
                        handler.AssignValueToVisual(visual, config[name].ToString());
                    }
                }
            }

            SavedByAdapter = false;
            return config;
        }

        private List<Visual> GetAllRecognizedVisuals(Visual visualTree)
        {
            var visuals = FetchAllVisuals(visualTree);
            var recognized = new List<Visual>();

            foreach (var visual in visuals)
            {
                if (_visualHandlers.Any(x => x.DoesMatchTo(visual)))
                {
                    if (_usePrefixes)
                    {
                        var name = ((FrameworkElement)visual).Name;
                        if (!PREFIXES.Any(x => x.StartsWith(name)))
                            continue;
                    }
                    recognized.Add(visual);
                }
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