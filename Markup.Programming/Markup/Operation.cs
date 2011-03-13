using System.Windows;
using Markup.Programming.Core;

namespace Markup.Programming
{
    public static class Operation
    {
        public static HandlerCollection GetAttach(DependencyObject obj)
        {
            var value = (HandlerCollection)obj.GetValue(AttachProperty);
            if (value == null) value = new HandlerCollection();
            obj.SetValue(AttachProperty, value);
            return value;
        }

        internal static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached("InternalAttach", typeof(HandlerCollection), typeof(Operation),
             new PropertyMetadata(null, Operation.OnAttachChanged));

        private static void OnAttachChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var parent = sender as DependencyObject;
            var child = e.NewValue as HandlerCollection;
            child.Attach(parent);
        }
    }
}
