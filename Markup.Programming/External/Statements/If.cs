using System.Windows;
using System.Windows.Markup;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// The If statement evaluates Value as a boolean
    /// and if it evalutes to true then it executes the Then statements
    /// otherwise it executes the Else statements.
    /// </summary>
    [ContentProperty("Value")]
    public class If : UntypedValueStatement
    {
        public If()
        {
            Then = new StatementCollection();
            Else = new StatementCollection();
        }

        public StatementCollection Then
        {
            get { return (StatementCollection)GetValue(ThenProperty); }
            set { SetValue(ThenProperty, value); }
        }

        public static readonly DependencyProperty ThenProperty =
            DependencyProperty.Register("Then", typeof(StatementCollection), typeof(If), null);

        public StatementCollection Else
        {
            get { return (StatementCollection)GetValue(ElseProperty); }
            set { SetValue(ElseProperty, value); }
        }

        public static readonly DependencyProperty ElseProperty =
            DependencyProperty.Register("Else", typeof(StatementCollection), typeof(If), null);

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(ValueProperty, ThenProperty, ElseProperty);
        }

        protected override void OnExecute(Engine engine)
        {
            if ((bool)engine.Evaluate(ValueProperty, Path, CodeTree, typeof(bool)))
                Then.Execute(engine);
            else
                Else.Execute(engine);
        }
    }
}
