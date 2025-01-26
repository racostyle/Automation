using System.Windows.Controls;
using System.Windows.Media;

namespace Automation.ConfigurationAdapter
{
    public class Handler_CheckBox : IVisualHandler
    {
        public void AssignValueToVisual(Visual visual, string value)
        {
            if (DoesMatchTo(visual))
            {
                ((CheckBox)visual).IsChecked = value.Equals("True", StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool DoesMatchTo(Visual visual)
        {
            return visual is CheckBox;
        }

        public string GetVisualNameWithoutPrefix(Visual visual)
        {
            return DoesMatchTo(visual) ? new string(((CheckBox)visual).Name.SkipWhile(x => char.IsLower(x)).ToArray()) : "";
        }

        public string GetVisualValue(Visual visual)
        {
            return DoesMatchTo(visual) ? ((CheckBox)visual).IsChecked.ToString()! : "";
        }
    }
}