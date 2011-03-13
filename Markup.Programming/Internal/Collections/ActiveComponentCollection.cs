using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;

namespace Markup.Programming.Core
{
    /// <summary>
    /// An ActiveComponentCollection is a collection of ActiveComponent items.
    /// </summary>
#if DEBUG
    [DebuggerDisplay("ActiveComponents = {Count}"), DebuggerTypeProxy(typeof(CollectionDebugView))]
#endif
    public class ActiveComponentCollection : ComponentCollection<ActiveComponent>
    {
    }
}
