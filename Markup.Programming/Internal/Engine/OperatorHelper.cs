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
        private static Dictionary<Operator, string> operatorMap = new Dictionary<Operator, string>
        {
            { Operator.Plus, "op_Addition" },
            { Operator.Minus, "op_Subtraction" },
            { Operator.Times, "op_Multiplication" },
            { Operator.Mod, "op_Modulus" },
            { Operator.Divide, "op_Division" },
            { Operator.Equals, "op_Equality" },
            { Operator.NotEquals, "op_Inequality" },
            { Operator.LessThan, "op_LessThan" },
            { Operator.LessThanOrEqual, "op_LessThanOrEqual" },
            { Operator.GreaterThan, "op_GreaterThan" },
            { Operator.GreaterThanOrEqual, "op_GreaterThanOrEqual" },
            { Operator.Negate, "op_UnaryNegation" },
        };

        public static object Evaluate(Engine engine, Operator op, ExpressionOrValue[] expressions)
        {
            // Get arity.
            var arity = GetArity(op);
            var n = expressions.Length;

            // Handler operators that require conditional evaluation.
            if (op == Operator.AndAnd || op == Operator.OrOr)
            {
                // Short-circuit evaluation.
                foreach (var expression in expressions)
                {
                    var value = TypeHelper.ConvertToBool(expression.Evaluate(engine));
                    if (op == Operator.AndAnd && !value) return false;
                    if (op == Operator.OrOr && value) return true;
                }
                return op == Operator.AndAnd;
            }
            if (op == Operator.Conditional)
            {
                if (n != 3) InvalidOperation(engine, op, n);
                var condition = TypeHelper.ConvertToBool(expressions[0].Evaluate(engine));
                return expressions[condition ? 1 : 2].Evaluate(engine);
            }

            // Remaining operators use unconditional evaluation.
            var operands = expressions.Select(expression => expression.Evaluate(engine)).ToArray();

            // Handle n-ary operators.
            switch (op)
            {
                case Operator.Comma:
                    return operands.LastOrDefault();
                case Operator.Format:
                    return string.Format(operands[0] as string, operands.Skip(1).ToArray());
                case Operator.GetProperty:
                    return PathHelper.GetProperty(engine, operands[0], operands[1] as string);
                case Operator.SetProperty:
                    { PathHelper.SetProperty(engine, operands[0], operands[1] as string, operands[2]); return operands[2]; }
                case Operator.GetItem:
                    return PathHelper.GetItem(engine, operands[0], operands.Skip(1).ToArray());
                case Operator.SetItem:
                    return PathHelper.SetItem(engine, operands[0], operands.Skip(1).ToArray());
                case Operator.ToArray:
                    return TypeHelper.CreateArray(operands);
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
                    case Operator.ToString:
                        return operands[0].ToString();
                    case Operator.IsNull:
                        return operands[0] == null;
                    case Operator.NotIsNull:
                        return operands[0] != null;
                    case Operator.IsZero:
                        return Op(engine, Operator.Equals, operands[0], 0);
                    case Operator.NotIsZero:
                        return Op(engine, Operator.NotEquals, operands[0], 0);
                    case Operator.GreaterThanZero:
                        return Op(engine, Operator.GreaterThan, operands[0], 0);
                    case Operator.GreaterThanOrEqualToZero:
                        return Op(engine, Operator.GreaterThanOrEqual, operands[0], 0);
                    case Operator.LessThanZero:
                        return Op(engine, Operator.LessThan, operands[0], 0);
                    case Operator.LessThanOrEqualToZero:
                        return Op(engine, Operator.LessThanOrEqual, operands[0], 0);
                    default:
                        break;
                }

                // Handle standard unary operators.
                return Op(engine, op, operands[0]);
            }

            // Handle binary operators.
            if (n < 2) InvalidOperation(engine, op, n);

            // Handle binary operators that aren't type transitive.
            if (!IsTypeTransitive(op) && n != 2) InvalidOperation(engine, op, n);

            // Handle special binary operators.
            switch (op)
            {
                case Operator.Is:
                    return Is(engine, operands[0], operands[1]);
                case Operator.As:
                    return Is(engine, operands[0], operands[1]) ? operands[0] : null;
                case Operator.Equate:
                    return operands[0].Equals(operands[1]);
                case Operator.Compare:
                    return (operands[0] as IComparable).CompareTo(operands[2]);
                default:
                    break;
            }

            // Process binary operations associating from the left.
            var result = Op(engine, op, operands[0], operands[1]);
            for (int i = 2; i < n; i++)
                result = Op(engine, op, result, operands[i]);
            return result;
        }

        public static int GetArity(Operator op)
        {
            switch (op)
            {
                case Operator.Not:
                case Operator.Negate:
                case Operator.BitwiseNot:
                case Operator.IsNull:
                case Operator.NotIsNull:
                case Operator.IsZero:
                case Operator.NotIsZero:
                case Operator.ToString:
                    return 1;
                case Operator.SetItem:
                    return 3;
                case Operator.Format:
                case Operator.GetItem:
                    return 0;
                default:
                    return 2;
            }
        }

        private static bool IsTypeTransitive(Operator op)
        {
            switch (op)
            {
                case Operator.Plus:
                case Operator.Minus:
                case Operator.Times:
                case Operator.Mod:
                case Operator.Divide:
                case Operator.AndAnd:
                case Operator.OrOr:
                case Operator.And:
                case Operator.Or:
                case Operator.BitwiseAnd:
                case Operator.BitwiseOr:
                case Operator.BitwiseXor:
                case Operator.LeftShift:
                case Operator.RightShift:
                case Operator.GetProperty:
                    return true;
            }
            return false;
        }

        private static object Op(Engine engine, Operator op, object operand)
        {
            var result = InvokeOperator(engine, op, operand);
            engine.Trace(TraceFlags.Operator, "Evaluate: {0} {1} => {2}", operand, op, result);
            return result;
        }

        private static object Op(Engine engine, Operator op, object lhs, object rhs)
        {
            var result = InvokeOperator(engine, op, lhs, rhs);
            engine.Trace(TraceFlags.Operator, "Evaluate: {0} {1} {2} => {3}", lhs, op, rhs, result);
            return result;
        }

        private static object InvokeOperator(Engine engine, Operator op, object operand)
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

        private static object InvokeOperator(Engine engine, Operator op, object lhs, object rhs)
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
                    rhs = TypeHelper.Convert(type, rhs);
                    return methodInfo.Invoke(null, new object[] { lhs, rhs });
                }
            }

            // Try our builtin methods.
            {
                var methodInfo = typeof(OperatorHelper).GetMethod(op.ToString(), new Type[] { type, type });
                if (methodInfo != null)
                {
                    rhs = TypeHelper.Convert(type, rhs);
                    return methodInfo.Invoke(null, new object[] { lhs, rhs });
                }
            }

            // Special case for equals.
            if (op == Operator.Equals && TypeHelper.IsClassObject(lhs))
                return lhs.Equals(rhs);

            return engine.Throw("no such operator: {0}({1}, {2})", op, type, type);
        }

        private static void InvalidOperation(Engine engine, Operator op, int n)
        {
            engine.Throw("invalid operator: {0}, operand count: {1}", op, n);
        }

        private static bool Is(Engine engine, object instance, object type)
        {
            if (type == null) engine.Throw("type");
            if (instance == null) return false;
            return (type as Type).IsAssignableFrom(instance.GetType());
        }

        public static double Plus(double lhs, double rhs) { return lhs + rhs; }
        public static double Minus(double lhs, double rhs) { return lhs - rhs; }
        public static double Times(double lhs, double rhs) { return lhs * rhs; }
        public static double Mod(double lhs, double rhs) { return lhs % rhs; }
        public static double Divide(double lhs, double rhs) { return lhs / rhs; }
        public static double Negate(double operand) { return -operand; }
        public static bool Equals(double lhs, double rhs) { return lhs == rhs; }
        public static bool NotEquals(double lhs, double rhs) { return lhs != rhs; }
        public static bool LessThan(double lhs, double rhs) { return lhs < rhs; }
        public static bool LessThanOrEquals(double lhs, double rhs) { return lhs <= rhs; }
        public static bool GreaterThan(double lhs, double rhs) { return lhs > rhs; }
        public static bool GreaterThanOrEquals(double lhs, double rhs) { return lhs >= rhs; }

        public static float Plus(float lhs, float rhs) { return lhs + rhs; }
        public static float Minus(float lhs, float rhs) { return lhs - rhs; }
        public static float Times(float lhs, float rhs) { return lhs * rhs; }
        public static float Mod(float lhs, float rhs) { return lhs % rhs; }
        public static float Divide(float lhs, float rhs) { return lhs / rhs; }
        public static float Negate(float operand) { return -operand; }
        public static bool Equals(float lhs, float rhs) { return lhs == rhs; }
        public static bool NotEquals(float lhs, float rhs) { return lhs != rhs; }
        public static bool LessThan(float lhs, float rhs) { return lhs < rhs; }
        public static bool LessThanOrEquals(float lhs, float rhs) { return lhs <= rhs; }
        public static bool GreaterThan(float lhs, float rhs) { return lhs > rhs; }
        public static bool GreaterThanOrEquals(float lhs, float rhs) { return lhs >= rhs; }

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
        public static bool LessThanOrEquals(long lhs, long rhs) { return lhs <= rhs; }
        public static bool GreaterThan(float lhs, long rhs) { return lhs > rhs; }
        public static bool GreaterThanOrEquals(long lhs, long rhs) { return lhs >= rhs; }

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
        public static bool LessThanOrEquals(int lhs, int rhs) { return lhs <= rhs; }
        public static bool GreaterThan(int lhs, int rhs) { return lhs > rhs; }
        public static bool GreaterThanOrEquals(int lhs, int rhs) { return lhs >= rhs; }

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
        public static bool LessThanOrEquals(short lhs, short rhs) { return lhs <= rhs; }
        public static bool GreaterThan(short lhs, short rhs) { return lhs > rhs; }
        public static bool GreaterThanOrEquals(short lhs, short rhs) { return lhs >= rhs; }

        public static bool And(bool lhs, bool rhs) { return lhs & rhs; }
        public static bool Or(bool lhs, bool rhs) { return lhs | rhs; }
        public static bool Not(bool operand) { return !operand; }
    }
}
