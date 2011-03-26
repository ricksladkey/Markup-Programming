using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// The While statement repeatedly evaluates its body until Value
    /// converted to a boolean returns false or the Break statement
    /// is issued in the body.
    /// </summary>
    public class While : UntypedValueBlock
    {
        protected override void OnExecute(Engine engine)
        {
            engine.SetBreakFrame();
            while (true)
            {
                if (Value != null)
                {
                    var value = (bool)engine.Get(ValueProperty, Path, CodeTree, typeof(bool));
                    if (!value) break;
                }
                Body.Execute(engine);
                engine.ClearShouldContinue();
                if (engine.ShouldInterrupt) break;
            }
        }
   }
}
