using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming.Core
{
    /// <summary>
    /// An IStatement is an IProcessor that performs an operation
    /// without a result.
    /// </summary>
    public interface IStatement : IProcessor
    {
        void Execute(Engine engine);
    }
}
