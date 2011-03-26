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
    /// The ForEach statement sets VariableName to each item in the
    /// collection specified by Value optionally converting the item
    /// to type Type and then executes its body.  The Break statement
    /// can be used to break out of the loop.  VariableName goes
    /// out of scope after the statement.
    /// </summary>
    public class ForEach : VariableBlock
    {
        protected override void OnExecute(Engine engine)
        {
            var type = engine.GetType(TypeProperty, TypePath, TypeCodeTree);
            var value = engine.Get(ValueProperty, Path, CodeTree) as IEnumerable;
            var name = engine.GetVariable(Var, VarCodeTree);
            foreach (object item in value)
            {
                engine.DefineVariable(name, TypeHelper.Convert(item, type));
                Body.Execute(engine);
                engine.ClearShouldContinue();
                if (engine.ShouldInterrupt) break;
            }
        }
    }
}
