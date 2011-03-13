using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming.Core
{
    /// <summary>
    /// An ExpressionOrValue is either an expression that can
    /// be evaluated or an ordinary value.  It is immutable.
    /// </summary>
    public struct ExpressionOrValue
    {
        public static ExpressionOrValue[] ValueArray(IEnumerable<object> values)
        {
            return values.Select(value => new ExpressionOrValue(null, value)).ToArray();
        }

        public static ExpressionOrValue[] ExpressionArray(IEnumerable<IExpression> expressions)
        {
            return expressions.Select(expression => new ExpressionOrValue(expression, null)).ToArray();
        }

        public static ExpressionOrValue[] ExpressionArray(ExpressionCollection expressions)
        {
            return expressions.Select(expression => new ExpressionOrValue(expression, null)).ToArray();
        }

        public IExpression expression;
        public object value;

        public ExpressionOrValue(IExpression expression, object value)
        {
            this.expression = expression;
            this.value = value;
        }

        public IExpression Expression { get { return expression; } }
        public object Value { get { return value; } }

        public object Evaluate(Engine engine)
        {
            return (expression != null) ? expression.Evaluate(engine) : value;
        }
    }
}
