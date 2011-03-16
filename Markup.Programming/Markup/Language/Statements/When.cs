using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// The When statement evaluates its body once when Value converted
    /// to a boolean evaluates to true.
    /// </summary>
    public class When : Block
    {
        protected override void OnExecute(Engine engine)
        {
            if (Body.Count == 0) engine.Throw("missing condition");
            var expression = Body[0] as IExpression;
            if (expression == null) engine.Throw("missing expression");
            if (!TypeHelper.ConvertToBool(expression.Evaluate(engine))) return;
            Body.ExecuteSkipOne(engine);
        }
    }
}
