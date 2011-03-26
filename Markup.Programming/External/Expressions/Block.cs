using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using Markup.Programming.Core;
using System.Windows;

namespace Markup.Programming
{
    [ContentProperty("Body")]
    public class Block : BlockExpression
    {
        protected override object OnGet(Engine engine)
        {
            engine.SetReturnFrame();
            Body.Execute(engine);
            return engine.GetAndResetReturnValue();
        }
    }
}
