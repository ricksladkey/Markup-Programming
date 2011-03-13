﻿using System;
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
    public class Op : ArgumentsExpressionWithType
    {
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(Op), null);

        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(Op), null);

        public object Value1
        {
            get { return (object)GetValue(Value1Property); }
            set { SetValue(Value1Property, value); }
        }

        public static readonly DependencyProperty Value1Property =
            DependencyProperty.Register("Value1", typeof(object), typeof(Op), null);

        public string Path1
        {
            get { return (string)GetValue(Path1Property); }
            set { SetValue(Path1Property, value); }
        }

        public static readonly DependencyProperty Path1Property =
            DependencyProperty.Register("Path1", typeof(string), typeof(Op), null);

        public Operator Operator
        {
            get { return (Operator)GetValue(OperatorProperty); }
            set { SetValue(OperatorProperty, value); }
        }

        public static readonly DependencyProperty OperatorProperty =
            DependencyProperty.Register("Operator", typeof(Operator), typeof(Op), null);

        public object Value2
        {
            get { return (object)GetValue(Value2Property); }
            set { SetValue(Value2Property, value); }
        }

        public static readonly DependencyProperty Value2Property =
            DependencyProperty.Register("Value2", typeof(object), typeof(Op), null);

        public string Path2
        {
            get { return (string)GetValue(Path2Property); }
            set { SetValue(Path2Property, value); }
        }

        public static readonly DependencyProperty Path2Property =
            DependencyProperty.Register("Path2", typeof(string), typeof(Op), null);

        protected override object OnEvaluate(Engine engine)
        {
            if (Operator == 0) ThrowHelper.Throw("missing operator");
            var type = engine.EvaluateType(TypeProperty, TypeName);
            if (Arguments.Count == 0)
            {
                var arity = Operator.GetArity();
                if (arity == 1)
                    return engine.Evaluate(Operator, engine.Evaluate(ValueProperty, Path, type));
                var value1 = engine.Evaluate(Value1Property, Path1, type);
                var value2 = engine.Evaluate(Value2Property, Path2, type);
                return engine.Evaluate(Operator, value1, value2);
            }
            return engine.Evaluate(Operator, Arguments);
        }
    }
}
