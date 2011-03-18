using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// An AssignmentOperator represents the corresponding C# operator.
    /// </summary>
    public enum AssignmentOperator
    {
        Assign = 0,

        PlusEquals = Op.Plus,
        MinusEquals = Op.Minus,
        TimesEquals = Op.Times,
        ModEquals = Op.Mod,
        DivideEquals = Op.Divide,
        AndEquals = Op.And,
        OrEquals = Op.Or,
        BitwiseAndEquals = Op.BitwiseAnd,
        BitwiseOrEquals = Op.BitwiseOr,
        BitwiseXor = Op.BitwiseXor,

        Increment = 1000,
        Decrement,
    }
}
