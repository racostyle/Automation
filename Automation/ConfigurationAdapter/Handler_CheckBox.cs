using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace Automation.ConfigurationAdapter
{
    public class Handler_CheckBox<T> : IVisualHandler where T : CheckBox
    {
        public void AssignValueToVisual(Visual visual, string value)
        {
            if (DoesMatchTo(visual))
            {
                ((T)visual).IsChecked = value.Equals("True", StringComparison.OrdinalIgnoreCase);
            }
        }

        public string GetVisualValue(Visual visual)
        {
            return DoesMatchTo(visual) ? ((T)visual).IsChecked.ToString()! : "";
        }

        public bool DoesMatchTo(Visual visual)
        {
            return visual is T;
        }

        public string GetVisualNameWithoutPrefix(Visual visual)
        {
            return DoesMatchTo(visual) ? new string(((T)visual).Name.SkipWhile(x => char.IsLower(x)).ToArray()) : "";
        }

        public string GetVisualName(Visual visual)
        {
            return DoesMatchTo(visual) ? ((T)visual).Name : "";
        }
    }
}