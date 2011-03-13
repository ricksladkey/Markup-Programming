using System.Windows;
using Markup.Programming.Core;

namespace Markup.Programming
{
    public static class Attach
    {
        public static HandlerCollection GetOperations(DependencyObject obj)
        {
            var value = (HandlerCollection)obj.GetValue(OperationsProperty);
            if (value == null) value = new HandlerCollection();
            obj.SetValue(OperationsProperty, value);
            return value;
        }

        internal static readonly DependencyProperty OperationsProperty =
            DependencyProperty.RegisterAttached("InternalOperations", typeof(HandlerCollection), typeof(Attach),
             new PropertyMetadata(null, Attach.OnOperationsChanged));

        private static void OnOperationsChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var parent = sender as DependencyObject;
            var child = e.NewValue as HandlerCollection;
            child.Attach(parent);
        }
    }
}
