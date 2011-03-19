using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// An InlineFunction is an expression whose body consists
    /// of statements.  To return a value for the expression
    /// use the Return statement.
    /// </summary>
    public class InlineFunction : BlockExpression
    {
        protected override object OnEvaluate(Engine engine)
        {
            engine.SetReturnFrame();
            Body.Execute(engine);
            return engine.GetAndResetReturnValue();
        }
    }
}
