using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Markup.Programming.Core;
using System.Windows;

namespace Markup.Programming
{
    /// <summary>
    /// A TypeVal expression returns a Value of type System.Type.
    /// </summary>
    public class TypeVal : ExpressionBase
    {
        public Type Value
        {
            get { return (Type)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(Type), typeof(TypeVal), null);

        public string TypeName
        {
            get { return (string)GetValue(TypeNameProperty); }
            set { SetValue(TypeNameProperty, value); }
        }

        public static readonly DependencyProperty TypeNameProperty =
            DependencyProperty.Register("TypeName", typeof(string), typeof(TypeVal), null);
        
        protected override object OnEvaluate(Engine engine)
        {
            return engine.EvaluateType(ValueProperty, TypeName);
        }
    }
}
