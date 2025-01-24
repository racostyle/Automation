using System.Windows.Controls;
using System.Windows.Media;

namespace Automation.ConfigurationAdapter
{
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