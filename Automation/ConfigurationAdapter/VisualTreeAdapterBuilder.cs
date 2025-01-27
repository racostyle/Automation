namespace Automation.ConfigurationAdapter
{
    public class VisualTreeAdapterBuilder
    {
        private List<IVisualHandler> _visualHandlers;

        public VisualTreeAdapterBuilder()
        {
            _visualHandlers = new List<IVisualHandler>();
        }

        public VisualTreeAdapterBuilder Configure_HandleTextBox()
        {
            _visualHandlers.Add(new Handler_TextBox());
            return this;
        }

        public VisualTreeAdapterBuilder Configure_HandleCheckBox()
        {
            _visualHandlers.Add(new Handler_CheckBox());
            return this;
        }

        public VisualTreeAdapter Build()
        {
            return new VisualTreeAdapter(_visualHandlers.ToArray());
        }

    }
}