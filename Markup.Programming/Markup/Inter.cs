#if !INTERACTIVITY

using System.Windows;
using Markup.Programming.Core;

namespace Markup.Programming
{
    public static class Inter
    {
        public static ActiveComponentCollection GetActive(DependencyObject obj)
        {
            var value = (ActiveComponentCollection)obj.GetValue(ActiveProperty);
            if (value == null) value = new ActiveComponentCollection();
            obj.SetValue(ActiveProperty, value);
            return value;
        }

        internal static readonly DependencyProperty ActiveProperty =
            DependencyProperty.RegisterAttached("InternalActive", typeof(ActiveComponentCollection), typeof(Inter),
             new PropertyMetadata(null, Inter.OnActiveChanged));

        private static void OnActiveChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var parent = sender as DependencyObject;
            var child = e.NewValue as ActiveComponentCollection;
            child.Attach(parent);
        }
    }
}

#else

using System.Windows;
using System.Windows.Interactivity; // portable
using TriggerCollection = System.Windows.Interactivity.TriggerCollection; // portable

namespace Markup.Programming
{
    public static class Inter
    {
        public static TriggerCollection GetActive(DependencyObject obj)
        {
            return Interaction.GetTriggers(obj);
        }
    }
}

#endif
