using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming.Core
{
    /// <summary>
    /// An IHiddenExpression is an IProcessor that produces a value
    /// but than should not be visible in an ordinary expression
    /// context.
    /// </summary>
    public interface IHiddenExpression : IProcessor
    {
        object Get(Engine engine);
    }
}
