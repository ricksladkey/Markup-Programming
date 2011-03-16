using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming.Core
{
    /// <summary>
    /// An IProcessor is an IComponent that has a processing operation
    /// that might or might not produce a meaningful value.  Both statements
    /// and expressions are processors.
    /// </summary>
    public interface IProcessor : IComponent
    {
        object Process(Engine engine);
    }
}
