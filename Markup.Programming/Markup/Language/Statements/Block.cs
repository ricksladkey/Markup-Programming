using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using Markup.Programming.Core;
using System.Windows;

namespace Markup.Programming
{
    /// <summary>
    /// A Block statement executes its body.  It can be used to supply
    /// multiple statements where one statement is expected or it can
    /// serve as a scope for parameters defined by the Set statement.
    /// </summary>
    [ContentProperty("Body")]
    public class Block : Statement
    {
        public Block()
        {
            Body = new StatementCollection();
        }

        public StatementCollection Body
        {
            get { return (StatementCollection)GetValue(BodyProperty); }
            set { SetValue(BodyProperty, value); }
        }

        public static readonly DependencyProperty BodyProperty =
            DependencyProperty.Register("Body", typeof(StatementCollection), typeof(Block), null);

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(BodyProperty);
        }

        protected override void OnExecute(Engine engine)
        {
            Body.Execute(engine);
        }
    }
}
