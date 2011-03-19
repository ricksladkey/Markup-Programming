using System.Windows;
using System.Windows.Markup;

namespace Markup.Programming.Core
{
    /// <summary>
    /// A StatementBlock executes its body.  It can be used to supply
    /// multiple statements where one statement is expected or it can
    /// serve as a scope for parameters defined by the Set statement.
    /// </summary>
    [ContentProperty("Body")]
    public abstract class StatementBlock : Statement
    {
        public StatementBlock()
        {
            Body = new StatementCollection();
        }

        public StatementCollection Body
        {
            get { return (StatementCollection)GetValue(BodyProperty); }
            set { SetValue(BodyProperty, value); }
        }

        public static readonly DependencyProperty BodyProperty =
            DependencyProperty.Register("Body", typeof(StatementCollection), typeof(StatementBlock), null);

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(BodyProperty);
        }
    }
}
