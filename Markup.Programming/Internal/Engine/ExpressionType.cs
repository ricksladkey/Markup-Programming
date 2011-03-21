using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming.Core
{
    [Flags]
    public enum ExpressionType
    {
        Standard = 0x00,
        Set = 0x01,
        Call = 0x02,
        Script = 0x04,
    }
}
