using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming.Core
{
    /// <summary>
    /// The IDynamicObject interface is a very simple
    /// interface for dynamic objects.
    /// </summary>
    public interface IDynamicObject
    {
        IEnumerable<NameTypePair> DynamicProperties { get; }
        object this[string propertyName] { get; set; }
    }
}
