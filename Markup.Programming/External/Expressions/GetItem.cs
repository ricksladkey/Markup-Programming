using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// The Item expression returns the item with indices specified
    /// by its content expression arguments, if any, or Index for
    /// context optionally converting index to type Type.
    /// </summary>
    public class GetItem : ItemAccessor
    {
        protected override object OnGet(Engine engine)
        {
            return GetItem(engine);
        }
    }
}
