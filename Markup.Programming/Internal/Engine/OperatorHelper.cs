using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Reflection;

namespace Markup.Programming.Core
{
    /// <summary>
    /// The OperatorHelper class is used to perform operations
    /// the same way that the C# language would.
    /// </summary>
    public static class OperatorHelper
    {
        public static object Operator(Engine engine, Op op, ExpressionOrValue[] expressions)
        {
            // Get arity.
            var arity = GetArity(op);
            var n = expressions.Length;

            // Handler operators that require conditional evaluation.
            if (op == Op.AndAnd || op == Op.OrOr)
            {
                // Short-circuit evaluation.
                foreach (var expression in expressions)
                {
                    var value = TypeHelper.ConvertToBool(expression.Get(engine));
                    if (op == Op.AndAnd && !value) return false;
                    if (op == Op.OrOr && value) return true;
                }
                return op == Op.AndAnd;
            }
            if (op == Op.Conditional)
            {
                if (n != 3) InvalidOperation(engine, op, n);
                var condition = TypeHelper.ConvertToBool(expressions[0].Get(engine));
                return expressions[condition ? 1 : 2].Get(engine);
            }

            // Remaining operators use unconditional evaluation.
            var operands = expressions.Select(expression => expression.Get(engine)).ToArray();

            // Handle n-ary operators.
            switch (op)
            {
                case Op.Comma:
                    return operands.LastOrDefault();
                case Op.Format:
                    return string.Format(operands[0] as string, operands.Skip(1).ToArray());
                case Op.GetProperty:
                    return PathHelper.GetProperty(engine, operands[0], operands[1] as string);
                case Op.SetProperty:
                    { PathHelper.SetProperty(engine, operands[0], operands[1] as string, operands[2]); return operands[2]; }
                case Op.GetItem:
                    return PathHelper.GetItem(engine, operands[0], operands.Skip(1).ToArray());
                case Op.SetItem:
                    return PathHelper.SetItem(engine, operands[0], operands.Skip(1).ToArray());
                case Op.New:
                    return TypeHelper.CreateInstance(operands[0] as Type, operands.Skip(1).ToArray());
                case Op.FirstNonNull:
                    return operands.SkipWhile(operand => operand == null).FirstOrDefault();
                default:
                    break;
            }

            // Handle unary operators.
            if (arity == 1)
            {
                if (n != 1) InvalidOperation(engine, op, n);

                // Handle special unary operators.
                switch (op)
                {
                    case Op.ToString:
                        return operands[0].ToString();
                    case Op.IsNull:
                        return operands[0] == null;
                    case Op.NotIsNull:
                        return operands[0] != null;
                    case Op.IsZero:
                        return InvokeOperator(engine, Op.Equals, operands[0], 0);
                    case Op.NotIsZero:
                        return InvokeOperator(engine, Op.NotEquals, operands[0], 0);
                    case Op.GreaterThanZero:
                        return InvokeOperator(engine, Op.GreaterThan, operands[0], 0);
                    case Op.GreaterThanOrEqualToZero:
                        return InvokeOperator(engine, Op.GreaterThanOrEqual, operands[0], 0);
                    case Op.LessThanZero:
                        return InvokeOperator(engine, Op.LessThan, operands[0], 0);
                    case Op.LessThanOrEqualToZero:
                        return InvokeOperator(engine, Op.LessThanOrEqual, operands[0], 0);
                    case Op.ToArray:
                        return TypeHelper.CreateArray(operands);
                    default:
                        break;
                }

                // Handle standard unary operators.
                return InvokeOperator(engine, op, operands[0]);
            }

            // Handle binary operators.
            if (n < 2) InvalidOperation(engine, op, n);

            // Handle binary operators that aren't type transitive.
            if (!IsTypeTransitive(op) && n != 2) InvalidOperation(engine, op, n);

            // Handle special binary operators.
            switch (op)
            {
                case Op.Is:
                    return Is(engine, operands[0], operands[1]);
                case Op.As:
                    return Is(engine, operands[0], operands[1]) ? operands[0] : null;
                case Op.Equate:
                    return Equate(engine, operands[0], operands[1]);
                case Op.Compare:
                    return Compare(engine, operands[0], operands[1]);
                case Op.Equals:
                case Op.NotEquals:
                    if (operands[0] == null || operands[1] == null)
                        return Equate(engine, operands[0], operands[1]) == (op == Op.Equals);
                    break;
                default:
                    break;
            }

            // Process binary operations associating from the left.
            var result = InvokeOperator(engine, op, operands[0], operands[1]);
            for (int i = 2; i < n; i++)
                result = InvokeOperator(engine, op, result, operands[i]);
            return result;
        }

        private static bool Is(Engine engine, object instance, object type)
        {
            if (type == null) engine.Throw("type");
            if (instance == null) return false;
            return (type as Type).IsAssignableFrom(instance.GetType());
        }

        private static bool Equate(Engine engine, object lhs, object rhs)
        {
            if (lhs == rhs) return true;
            if (lhs == null || rhs == null) return false;
            return lhs.Equals(rhs);
        }

        private static int Compare(Engine engine, object lhs, object rhs)
        {
            return (lhs as IComparable).CompareTo(rhs);
        }

        public static int GetArity(Op op)
        {
            switch (op)
            {
                case Op.Not:
                case Op.Negate:
                case Op.BitwiseNot:
                case Op.IsNull:
                case Op.NotIsNull:
                case Op.IsZero:
                case Op.NotIsZero:
                case Op.ToString:
                case Op.ToArray:
                    return 1;
                case Op.Format:
                case Op.GetItem:
                case Op.SetItem:
                case Op.New:
                case Op.FirstNonNull:
                    return 0;
                default:
                    return 2;
            }
        }

        public static int GetArity(AssignmentOp op)
        {
            switch (op)
            {
                case AssignmentOp.Increment:
                case AssignmentOp.Decrement:
                    return 1;
                default:
                    return 2;
            }
        }

        private static bool IsTypeTransitive(Op op)
        {
            switch (op)
            {
                case Op.Plus:
                case Op.Minus:
                case Op.Times:
                case Op.Mod:
                case Op.Divide:
                case Op.AndAnd:
                case Op.OrOr:
                case Op.And:
                case Op.Or:
                case Op.BitwiseAnd:
                case Op.BitwiseOr:
                case Op.BitwiseXor:
                case Op.LeftShift:
                case Op.RightShift:
                case Op.GetProperty:
                    return true;
            }
            return false;
        }

        private static object InvokeOperator(Engine engine, Op op, object operand)
        {
            var result = InvokeOperatorInternal(engine, op, operand);
            engine.Trace(TraceFlags.Operator, "Evaluate: {0} {1} => {2}", operand, op, result);
            return result;
        }

        private static object InvokeOperator(Engine engine, Op op, object lhs, object rhs)
        {
            var result = InvokeOperatorInternal(engine, op, lhs, rhs);
            engine.Trace(TraceFlags.Operator, "Evaluate: {0} {1} {2} => {3}", lhs, op, rhs, result);
            return result;
        }

        private static object InvokeOperatorInternal(Engine engine, Op op, object operand)
        {
            if (operand == null) engine.Throw("operand");
            var type = operand.GetType();

            // Try the operator map.
            if (operatorMap.ContainsKey(op))
            {
                var methodInfo = type.GetMethod(operatorMap[op], new Type[] { type });
                if (methodInfo != null)
                    return methodInfo.Invoke(null, new object[] { operand });
            }

            // Try our builtin methods.
            {
                var methodInfo = typeof(OperatorHelper).GetMethod(op.ToString(), new Type[] { type });
                if (methodInfo != null)
                {
                    return methodInfo.Invoke(null, new object[] { operand });
                }
            }

            return engine.Throw("no such operator: {0}({1})", op, type);
        }

        private static object InvokeOperatorInternal(Engine engine, Op op, object lhs, object rhs)
        {
            if (lhs == null) engine.Throw("lhs");
            if (rhs == null) engine.Throw("rhs");
            var type = lhs.GetType();

            // Try the operator map.
            if (operatorMap.ContainsKey(op))
            {
                var methodInfo = type.GetMethod(operatorMap[op], new Type[] { type, type });
                if (methodInfo != null)
                {
                    rhs = TypeHelper.Convert(rhs, type);
                    return methodInfo.Invoke(null, new object[] { lhs, rhs });
                }
            }

            // Try our builtin methods.
            {
                var methodInfo = typeof(OperatorHelper).GetMethod(op.ToString(), new Type[] { type, type });
                if (methodInfo != null)
                {
                    rhs = TypeHelper.Convert(rhs, type);
                    return methodInfo.Invoke(null, new object[] { lhs, rhs });
                }
            }

            // Special case for equals.
            if (op == Op.Equals || op == Op.NotEquals)
            {
                if (TypeHelper.IsClassObject(lhs) || type.IsEnum)
                    return lhs.Equals(rhs) == (op == Op.Equals);
            }

            return engine.Throw("no such operator: {0}({1}, {2})", op, type, type);
        }

        private static void InvalidOperation(Engine engine, Op op, int n)
        {
            engine.Throw("invalid operator: {0}, operand count: {1}", op, n);
        }

        private static Dictionary<Op, string> operatorMap = new Dictionary<Op, string>
        {
            { Op.Plus, "op_Addition" },
            { Op.Minus, "op_Subtraction" },
            { Op.Times, "op_Multiplication" },
            { Op.Mod, "op_Modulus" },
            { Op.Divide, "op_Division" },
            { Op.Equals, "op_Equality" },
            { Op.NotEquals, "op_Inequality" },
            { Op.LessThan, "op_LessThan" },
            { Op.LessThanOrEqual, "op_LessThanOrEqual" },
            { Op.GreaterThan, "op_GreaterThan" },
            { Op.GreaterThanOrEqual, "op_GreaterThanOrEqual" },
            { Op.Negate, "op_UnaryNegation" },
        };

        public static double Plus(double lhs, double rhs) { return lhs + rhs; }
        public static double Minus(double lhs, double rhs) { return lhs - rhs; }
        public static double Times(double lhs, double rhs) { return lhs * rhs; }
        public static double Mod(double lhs, double rhs) { return lhs % rhs; }
        public static double Divide(double lhs, double rhs) { return lhs / rhs; }
        public static double Negate(double operand) { return -operand; }
        public static bool Equals(double lhs, double rhs) { return lhs == rhs; }
        public static bool NotEquals(double lhs, double rhs) { return lhs != rhs; }
        public static bool LessThan(double lhs, double rhs) { return lhs < rhs; }
        public static bool LessThanOrEqual(double lhs, double rhs) { return lhs <= rhs; }
        public static bool GreaterThan(double lhs, double rhs) { return lhs > rhs; }
        public static bool GreaterThanOrEqual(double lhs, double rhs) { return lhs >= rhs; }

        public static float Plus(float lhs, float rhs) { return lhs + rhs; }
        public static float Minus(float lhs, float rhs) { return lhs - rhs; }
        public static float Times(float lhs, float rhs) { return lhs * rhs; }
        public static float Mod(float lhs, float rhs) { return lhs % rhs; }
        public static float Divide(float lhs, float rhs) { return lhs / rhs; }
        public static float Negate(float operand) { return -operand; }
        public static bool Equals(float lhs, float rhs) { return lhs == rhs; }
        public static bool NotEquals(float lhs, float rhs) { return lhs != rhs; }
        public static bool LessThan(float lhs, float rhs) { return lhs < rhs; }
        public static bool LessThanOrEqual(float lhs, float rhs) { return lhs <= rhs; }
        public static bool GreaterThan(float lhs, float rhs) { return lhs > rhs; }
        public static bool GreaterThanOrEqual(float lhs, float rhs) { return lhs >= rhs; }

        public static long Plus(long lhs, long rhs) { return lhs + rhs; }
        public static long Minus(long lhs, long rhs) { return lhs - rhs; }
        public static long Times(long lhs, long rhs) { return lhs * rhs; }
        public static long Mod(long lhs, long rhs) { return lhs % rhs; }
        public static long Divide(long lhs, long rhs) { return lhs / rhs; }
        public static long Negate(long operand) { return -operand; }
        public static long BitwiseAnd(long lhs, long rhs) { return lhs & rhs; }
        public static long BitwiseOr(long lhs, long rhs) { return lhs & rhs; }
        public static long BitwiseXor(long lhs, long rhs) { return lhs & rhs; }
        public static long BitwiseNot(long operand) { return ~operand; }
        public static long LeftShift(long lhs, long rhs) { return lhs << (int)rhs; }
        public static long RightShift(long lhs, long rhs) { return lhs >> (int)rhs; }
        public static bool Equals(long lhs, long rhs) { return lhs == rhs; }
        public static bool NotEquals(long lhs, long rhs) { return lhs != rhs; }
        public static bool LessThan(long lhs, long rhs) { return lhs < rhs; }
        public static bool LessThanOrEqual(long lhs, long rhs) { return lhs <= rhs; }
        public static bool GreaterThan(float lhs, long rhs) { return lhs > rhs; }
        public static bool GreaterThanOrEqual(long lhs, long rhs) { return lhs >= rhs; }

        public static int Plus(int lhs, int rhs) { return lhs + rhs; }
        public static int Minus(int lhs, int rhs) { return lhs - rhs; }
        public static int Times(int lhs, int rhs) { return lhs * rhs; }
        public static int Mod(int lhs, int rhs) { return lhs % rhs; }
        public static int Divide(int lhs, int rhs) { return lhs / rhs; }
        public static int Negate(int operand) { return -operand; }
        public static int BitwiseAnd(int lhs, int rhs) { return lhs & rhs; }
        public static int BitwiseOr(int lhs, int rhs) { return lhs & rhs; }
        public static int BitwiseXor(int lhs, int rhs) { return lhs & rhs; }
        public static int BitwiseNot(int operand) { return ~operand; }
        public static int LeftShift(int lhs, int rhs) { return lhs << rhs; }
        public static int RightShift(int lhs, int rhs) { return lhs >> rhs; }
        public static bool Equals(int lhs, int rhs) { return lhs == rhs; }
        public static bool NotEquals(int lhs, int rhs) { return lhs != rhs; }
        public static bool LessThan(int lhs, int rhs) { return lhs < rhs; }
        public static bool LessThanOrEqual(int lhs, int rhs) { return lhs <= rhs; }
        public static bool GreaterThan(int lhs, int rhs) { return lhs > rhs; }
        public static bool GreaterThanOrEqual(int lhs, int rhs) { return lhs >= rhs; }

        public static short Plus(short lhs, short rhs) { return (short)(lhs + rhs); }
        public static short Minus(short lhs, short rhs) { return (short)(lhs - rhs); }
        public static short Times(short lhs, short rhs) { return (short)(lhs * rhs); }
        public static short Mod(short lhs, short rhs) { return (short)(lhs % rhs); }
        public static short Divide(short lhs, short rhs) { return (short)(lhs / rhs); }
        public static short Negate(short operand) { return (short)-operand; }
        public static short BitwiseAnd(long lhs, short rhs) { return (short)(lhs & rhs); }
        public static short BitwiseOr(short lhs, short rhs) { return (short)(lhs & rhs); }
        public static short BitwiseXor(short lhs, short rhs) { return (short)(lhs & rhs); }
        public static short BitwiseNot(short operand) { return (short)~operand; }
        public static short LeftShift(short lhs, short rhs) { return (short)(lhs << rhs); }
        public static short RightShift(short lhs, short rhs) { return (short)(lhs >> rhs); }
        public static bool Equals(short lhs, short rhs) { return lhs == rhs; }
        public static bool NotEquals(short lhs, short rhs) { return lhs != rhs; }
        public static bool LessThan(short lhs, short rhs) { return lhs < rhs; }
        public static bool LessThanOrEqual(short lhs, short rhs) { return lhs <= rhs; }
        public static bool GreaterThan(short lhs, short rhs) { return lhs > rhs; }
        public static bool GreaterThanOrEqual(short lhs, short rhs) { return lhs >= rhs; }

        public static bool And(bool lhs, bool rhs) { return lhs & rhs; }
        public static bool Or(bool lhs, bool rhs) { return lhs | rhs; }
        public static bool Not(bool operand) { return !operand; }
    }
}
