﻿using System.Windows;
using Markup.Programming.Core;

namespace Markup.Programming
{
    public static class Attached
    {
        public static HandlerCollection GetOperations(DependencyObject obj)
        {
            var value = (HandlerCollection)obj.GetValue(OperationsProperty);
            if (value == null) value = new HandlerCollection();
            obj.SetValue(OperationsProperty, value);
            return value;
        }

        internal static readonly DependencyProperty OperationsProperty =
            DependencyProperty.RegisterAttached("InternalOperations", typeof(HandlerCollection), typeof(Attached),
             new PropertyMetadata(null, Attached.OnOperationsChanged));

        private static void OnOperationsChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var dependencyObject = sender as DependencyObject;
            var collection = e.NewValue as HandlerCollection;
            collection.AttachOperations(dependencyObject);
        }
    }
}