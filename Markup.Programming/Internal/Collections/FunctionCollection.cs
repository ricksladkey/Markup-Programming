using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Markup.Programming.Core
{
    /// <summary>
    /// A FunctionCollection is a collection of Functions
    /// </summary>
#if DEBUG
    [DebuggerDisplay("Functions = {Count}"), DebuggerTypeProxy(typeof(CollectionDebugView))]
#endif
    public class FunctionCollection : StatementCollectionBase<Function>
    {
    }
}
