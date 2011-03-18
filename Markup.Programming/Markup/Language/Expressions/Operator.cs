using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// The Op expression has three modes, unary mode, binary mode, params
    /// mode.  In unary mode, the expresion operator Value is evaluated.
    /// In binary mode, the expression Value1 Operator Value2 is evaluated.
    /// In params mode, Operator is applied to however many arguments
    /// are supplied in content of the Op expression.
    /// 
    /// The following special operators are notable:
    /// 
    /// - AndAnd: logical and with short curcuit evaluation
    /// - OrOr: logical or with short curcuit evaluation
    /// - Conditional: condition operation
    /// - Comma: evaluates both and returns the second
    /// - IsNull: true if the value is null
    /// - NotIsNull: true if the value is not null
    /// </summary>
    public class Operator : ArgumentsExpressionWithType
    {
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(Operator), null);

        public object Value1
        {
            get { return (object)GetValue(Value1Property); }
            set { SetValue(Value1Property, value); }
        }

        public static readonly DependencyProperty Value1Property =
            DependencyProperty.Register("Value1", typeof(object), typeof(Operator), null);

        public string Path1 { get; set; }

        private PathExpression pathExpression1 = new PathExpression();
        protected PathExpression PathExpression1 { get { return pathExpression1; } }

        public Op Op { get; set; }

        public object Value2
        {
            get { return (object)GetValue(Value2Property); }
            set { SetValue(Value2Property, value); }
        }

        public static readonly DependencyProperty Value2Property =
            DependencyProperty.Register("Value2", typeof(object), typeof(Operator), null);

        public string Path2 { get; set; }

        private PathExpression pathExpression2 = new PathExpression();
        protected PathExpression PathExpression2 { get { return pathExpression2; } }

        protected override object OnEvaluate(Engine engine)
        {
            if (Op == default(Op)) engine.Throw("missing operator");
            var type = engine.EvaluateType(TypeProperty, TypeName);
            if (Arguments.Count == 0)
            {
                var arity = Op.GetArity();
                if (arity == 1)
                    return engine.Evaluate(Op, engine.Evaluate(ValueProperty, Path, PathExpression, type));
                var value1 = engine.Evaluate(Value1Property, Path1, PathExpression1, type);
                var value2 = engine.Evaluate(Value2Property, Path2, PathExpression2, type);
                return engine.Evaluate(Op, value1, value2);
            }
            return engine.Evaluate(Op, Arguments);
        }
    }
}
