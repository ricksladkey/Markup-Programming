using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming.Core
{
    [Flags]
    public enum CodeType
    {
        Variable = 0x01,
        EventExpression = 0x02,
        GetExpression = 0x04,
        SetExpression = 0x08,
        CallExpression = 0x10,
        Script = 0x20,
    }
}
