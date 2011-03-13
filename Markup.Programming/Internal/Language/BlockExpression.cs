using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;

namespace Markup.Programming.Core
{
    /// <summary>
    /// A BlockExpression is an expression that has a Body property as its
    /// default content.  It is the base class for InlineFunction and
    /// Iterator.
    /// </summary>
    [ContentProperty("Body")]
    public abstract class BlockExpression : ExpressionBase
    {
        public BlockExpression()
        {
            Body = new StatementCollection();
        }

        public StatementCollection Body
        {
            get { return (StatementCollection)GetValue(BodyProperty); }
            set { SetValue(BodyProperty, value); }
        }

        public static readonly DependencyProperty BodyProperty =
            DependencyProperty.Register("Body", typeof(StatementCollection), typeof(BlockExpression), null);

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(BodyProperty);
        }
    }
}
