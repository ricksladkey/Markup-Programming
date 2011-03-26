using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Markup.Programming.Core;
using System.Windows;

namespace Markup.Programming
{
    /// <summary>
    /// The When statement evaluates its body once when Value converted
    /// to a boolean evaluates to true.
    /// </summary>
    public class When : UntypedValueBlock
    {
        protected override void OnExecute(Engine engine)
        {
            if (engine.HasBindingOrValue(ValueProperty, Path))
            {
                if ((bool)engine.Get(ValueProperty, Path, CodeTree, typeof(bool))) Body.Execute(engine);
                return;
            }
            if (Body.Count == 0) engine.Throw("missing condition");
            var expression = Body[0] as IExpression;
            if (expression == null) engine.Throw("missing expression");
            if (!TypeHelper.ConvertToBool(expression.Get(engine))) return;
            Body.ExecuteSkipOne(engine);
        }
    }
}
