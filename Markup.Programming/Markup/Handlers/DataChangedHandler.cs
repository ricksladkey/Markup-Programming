using System.Windows;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// The DataChangedHandler invokes its passive components when Value
    /// changes. If Value has a binding then normal property and collection
    /// change notification will cause the handler to be raised.  The event
    /// args will be of type DependencyPropertyChangedEventArgs.
    /// </summary>
    public class DataChangedHandler : Handler
    {
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(DataChangedHandler),
            new PropertyMetadata((s, e) => (s as DataChangedHandler).OnValueChanged(s, e)));

        private void OnValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ExecuteBody(sender, e);
        }
    }
}
