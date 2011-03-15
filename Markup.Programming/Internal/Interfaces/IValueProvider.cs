using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming.Core
{
    public interface IValueProvider
    {
        object Value { get; set;  }
    }
}
