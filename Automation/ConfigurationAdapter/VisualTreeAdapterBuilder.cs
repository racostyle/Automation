using System.Windows.Controls;

namespace Automation.ConfigurationAdapter
{
    public class VisualTreeAdapterBuilder
    {
        private List<IVisualHandler> _visualHandlers;
        private bool _usePrefixes = false;

        public VisualTreeAdapterBuilder()
        {
            _visualHandlers = new List<IVisualHandler>();
        }

        public void ConfigureToUsePrefixes()
        {
            _usePrefixes = true;
        }

        public VisualTreeAdapterBuilder Add_HandlerTextBox()
        {
            _visualHandlers.Add(new Handler_TextBox<TextBox>());
            return this;
        }

        public VisualTreeAdapterBuilder Add_HandlerCheckBox()
        {
            _visualHandlers.Add(new Handler_CheckBox<CheckBox>());
            return this;
        }

        public VisualTreeAdapterBuilder Add_HandlerTextBlock()
        {
            _visualHandlers.Add(new Handler_TextBlock<TextBlock>());
            return this;
        }


        public VisualTreeAdapter Build()
        {
            return new VisualTreeAdapter(_visualHandlers.ToArray(), _usePrefixes);
        }

    }
}