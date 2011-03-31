using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming.Core
{
    [Flags]
    public enum CodeType
    {
        Statement = 0x01,
        Variable = 0x02,
        Event = 0x04,
        Get = 0x08,
        Set = 0x10,
        Call = 0x20,
        Script = 0x40,
    }
}
