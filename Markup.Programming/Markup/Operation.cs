using System.Windows;
using Markup.Programming.Core;

namespace Markup.Programming
{
    public static class Operation
    {
        public static ActiveComponentCollection GetAttach(DependencyObject obj)
        {
            var value = (ActiveComponentCollection)obj.GetValue(AttachProperty);
            if (value == null) value = new ActiveComponentCollection();
            obj.SetValue(AttachProperty, value);
            return value;
        }

        internal static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached("InternalAttach", typeof(ActiveComponentCollection), typeof(Operation),
             new PropertyMetadata(null, Operation.OnAttachChanged));

        private static void OnAttachChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var parent = sender as DependencyObject;
            var child = e.NewValue as ActiveComponentCollection;
            child.Attach(parent);
        }
    }
}
