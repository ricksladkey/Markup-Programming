﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// An Op represents the corresponding C# operator
    /// or method.
    /// </summary>
    public enum Op
    {
        Plus = 1,
        Minus,
        Times,
        Mod,
        Divide,
        Negate,

        AndAnd,
        OrOr,

        And,
        Or,
        Not,

        BitwiseAnd,
        BitwiseOr,
        BitwiseXor,
        BitwiseNot,
        LeftShift,
        RightShift,

        Equals,
        NotEquals,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,

        Comma,
        Conditional,
        As,
        Is,
        Equate,
        Compare,
        ToArray,
        GetProperty,
        SetProperty,
        GetItem,
        SetItem,
        New,

        IsNull,
        NotIsNull,
        ToString,
        IsZero,
        NotIsZero,
        GreaterThanZero,
        LessThanZero,
        GreaterThanOrEqualToZero,
        LessThanOrEqualToZero,
        Format,
    }

    /// <summary>
    /// The OperatorExtensions class extends the Operator
    /// enumeration to to include methods to perform those operations.
    /// </summary>
    public static class OperatorExtensions
    {
        public static int GetArity(this Op op)
        {
            return OperatorHelper.GetArity(op);
        }
    }
}
