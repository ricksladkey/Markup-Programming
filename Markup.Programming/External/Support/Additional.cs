using System.ComponentModel;
using System.Windows;

namespace Markup.Programming
{
    public static class Additional
    {
        [TypeConverter(typeof(OperationsConverter))]
        public static OperationCollection GetOperations(DependencyObject obj)
        {
            return (OperationCollection)obj.GetValue(OperationsProperty);
        }

        public static void SetOperations(DependencyObject obj, OperationCollection value)
        {
            obj.SetValue(OperationsProperty, value);
        }

        public static readonly DependencyProperty OperationsProperty =
            DependencyProperty.RegisterAttached("Operations", typeof(OperationCollection), typeof(Additional), new PropertyMetadata(null, OnPropertyOperationsChanged));

        private static void OnPropertyOperationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var operations = Attached.GetOperations(d);
            if (e.NewValue == null) return;
            foreach (var operation in e.NewValue as OperationCollection) operations.Add(operation);
        }
    }
}
