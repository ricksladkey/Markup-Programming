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
        public struct DependencyPropertyChangedEventArgs
        {
            public DependencyPropertyChangedEventArgs(DependencyProperty property, object oldValue, object newValue)
            {
                this.property = property;
                this.oldValue = oldValue;
                this.newValue = newValue;
            }
            private object newValue;
            private object oldValue;
            private DependencyProperty property;
            public object NewValue { get { return newValue; } }
            public object OldValue { get { return oldValue; } }
            public DependencyProperty Property { get { return property; } }
        }

        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(DataChangedHandler),
            new PropertyMetadata((s, e) => (s as DataChangedHandler).OnValueChanged(s,
                new DependencyPropertyChangedEventArgs(e.Property, e.OldValue, e.NewValue))));

        private bool enabled;

        private void OnValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (enabled) EventHandler(sender, e);
        }

        protected override void OnActiveExecute(Engine engine)
        {
            enabled = true;
            if (Value != null) OnValueChanged(this, new DependencyPropertyChangedEventArgs(ValueProperty, Value, Value));
        }
    }
}
