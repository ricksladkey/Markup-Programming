using System.ComponentModel;
using System.Windows;
using Markup.Programming.Core;

namespace Markup.Programming
{
    public static class Attached
    {
        [TypeConverter(typeof(OperationsConverter))]
        public static HandlerCollection GetOperations(DependencyObject obj)
        {
            var value = obj.GetValue(OperationsProperty) as HandlerCollection;
            if (value == null) value = new HandlerCollection();
            obj.SetValue(OperationsProperty, value);
            return value;
        }

        public static void SetOperations(DependencyObject obj, HandlerCollection value)
        {
            var operations = GetOperations(obj);
            foreach (var handler in value) operations.Add(handler);
        }

        internal static readonly DependencyProperty OperationsProperty =
            DependencyProperty.RegisterAttached("HiddenOperations", typeof(HandlerCollection), typeof(Attached),
             new PropertyMetadata(null, OnPropertyOperationsChanged));

        private static void OnPropertyOperationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (e.NewValue as HandlerCollection).AttachOperations(d);
        }
    }
}
