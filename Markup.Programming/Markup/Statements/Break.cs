using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Markup.Programming.Core;
using System.Windows;

namespace Markup.Programming
{
    /// <summary>
    /// The Break statement breaks to the nearest enclosing loop such as
    /// a For, While or Iterator.
    /// </summary>
    public class Break : Statement
    {
        protected override void OnExecute(Engine engine)
        {
            engine.SetShouldBreak();
        }
    }
}
