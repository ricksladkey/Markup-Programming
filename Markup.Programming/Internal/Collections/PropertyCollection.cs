using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;

namespace Markup.Programming.Core
{
    /// <summary>
    /// A PropertyCollection is a collection of Property objects.
    /// </summary>
#if DEBUG
    [DebuggerDisplay("Properties = {Count}"), DebuggerTypeProxy(typeof(CollectionDebugView))]
#endif
    public class PropertyCollection : ComponentCollection<Property>
    {
    }
}
