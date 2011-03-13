using System.Windows;

namespace Markup.Programming.Core
{
    /// <summary>
    /// An UntypedValueBlock is a Block that also has Value and
    /// ValueParam properties but not Type.
    /// </summary>
    public abstract class UntypedValueBlock : Block
    {
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public string Path { get; set; }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(UntypedValueBlock), null);

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(ValueProperty);
        }
    }
}
