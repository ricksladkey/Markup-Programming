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

        PlusEquals = Operator.Plus,
        MinusEquals = Operator.Minus,
        TimesEquals = Operator.Times,
        ModEquals = Operator.Mod,
        DivideEquals = Operator.Divide,
        AndEquals = Operator.And,
        OrEquals = Operator.Or,
        BitwiseAndEquals = Operator.BitwiseAnd,
        BitwiseOrEquals = Operator.BitwiseOr,
        BitwiseXor = Operator.BitwiseXor,

        Increment = 1000,
        Decrement,
    }
}
