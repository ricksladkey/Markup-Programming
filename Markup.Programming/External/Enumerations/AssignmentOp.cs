﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// An AssignmentOp represents the corresponding C# operator.
    /// </summary>
    public enum AssignmentOp
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
        PostIncrement,
        PostDecrement,
    }

    /// <summary>
    /// The AssignmentOperatorExtensions class extends the Operator
    /// enumeration to to include methods to perform those operations.
    /// </summary>
    public static class AssignmentOperatorExtensions
    {
        public static int GetArity(this AssignmentOp op)
        {
            return OperatorHelper.GetArity(op);
        }
    }
}
