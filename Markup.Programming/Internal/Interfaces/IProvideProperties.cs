using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming.Core
{
    /// <summary>
    /// The IProvideProperties interface is a very simple
    /// interface for dynamic property types and objects.
    /// </summary>
    public interface IProvideProperties
    {
        IEnumerable<NameTypePair> PropertyInfo { get; }
        object this[string propertyName] { get; set; }
    }
}
