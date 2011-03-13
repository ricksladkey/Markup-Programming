using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Markup.Programming.Core
{
    /// <summary>
    /// A ParameterCollection is a collection of Parameter objects.
    /// Note that it is not a component.
    /// </summary>
#if DEBUG
    [DebuggerDisplay("Parameters = {Count}"), DebuggerTypeProxy(typeof(CollectionDebugView))]
#endif
    public class ParameterCollection : List<Parameter>
    {
    }
}
