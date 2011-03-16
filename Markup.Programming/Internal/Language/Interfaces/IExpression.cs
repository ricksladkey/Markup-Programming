using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming.Core
{
    /// <summary>
    /// An IExpression is an IProcessor produces a value.
    /// </summary>
    public interface IExpression : IProcessor
    {
        object Evaluate(Engine engine);
    }
}
