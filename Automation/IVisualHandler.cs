﻿using System.Windows.Controls;
using System.Windows.Media;

namespace Automation
{
    public interface IVisualHandler
    {
        void AssignValueToVisual(Visual visual, string value);
        string GetVisualValue(Visual visual);
        bool DoesMatchTo(Visual visual);
        string GetVisualNameWithoutPrefix(Visual visual);
    }
}