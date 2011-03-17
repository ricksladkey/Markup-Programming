using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// The ForEach statement sets ParameterName to each item in the
    /// collection specified by Value optionally converting the item
    /// to type Type and then executes its body.  The Break statement
    /// can be used to break out of the loop.  ParameterName goes
    /// out of scope after the statement.
    /// </summary>
    public class ForEach : ParameterBlock
    {
        protected override void OnExecute(Engine engine)
        {
            var type = engine.EvaluateType(TypeProperty, TypeName);
            var value = engine.Evaluate(ValueProperty, Path, PathExpression) as IEnumerable;
            var name = ParameterName;
            foreach (object item in value)
            {
                engine.DefineParameter(name, TypeHelper.Convert(item, type));
                Body.Execute(engine);
            }
        }
    }
}
