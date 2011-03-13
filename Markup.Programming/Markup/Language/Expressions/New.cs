using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Markup.Programming.Core;
using System.Windows;

namespace Markup.Programming
{
    /// <summary>
    /// The New expression calls the constructor for type Type, optionally
    /// qualified by generic type arguments TypeArguments, and supplied
    /// with constructor arguments which are the content of the New
    /// statement.
    /// </summary>
    public class New : ArgumentsExpression
    {
        public New()
        {
            TypeArguments = new ExpressionCollection();
        }

        public Type Type
        {
            get { return (Type)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(Type), typeof(New), null);

        public ExpressionCollection TypeArguments
        {
            get { return (ExpressionCollection)GetValue(TypeArgumentsProperty); }
            set { SetValue(TypeArgumentsProperty, value); }
        }

        public static readonly DependencyProperty TypeArgumentsProperty =
            DependencyProperty.Register("TypeArguments", typeof(ExpressionCollection), typeof(New), null);

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(TypeArgumentsProperty);
        }

        protected override object OnEvaluate(Engine engine)
        {
            var type = Type;
            if (TypeArguments.Count != 0)
                type = type.MakeGenericType(TypeArguments.Evaluate(engine).Cast<Type>().ToArray());
            return Activator.CreateInstance(type, Arguments.Evaluate(engine));
        }
    }
}
