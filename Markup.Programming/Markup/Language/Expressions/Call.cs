using System;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// The Call expression calls a method or a function.
    /// </summary>
    [ContentProperty("Arguments")]
    public class Call : ExpressionBase
    {
        public Call()
        {
            Arguments = new ExpressionCollection();
            TypeArguments = new ExpressionCollection();
        }

        public string Path { get; set; }

        public Type Type
        {
            get { return (Type)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(Type), typeof(Call), null);

        public string TypeName { get; set; }

        public object Parameter
        {
            get { return (object)GetValue(ParameterProperty); }
            set { SetValue(ParameterProperty, value); }
        }

        public static readonly DependencyProperty ParameterProperty =
            DependencyProperty.Register("Parameter", typeof(object), typeof(Call), null);

        public string ParameterPath { get; set; }

        public ExpressionCollection Arguments
        {
            get { return (ExpressionCollection)GetValue(ArgumentsProperty); }
            set { SetValue(ArgumentsProperty, value); }
        }

        public static readonly DependencyProperty ArgumentsProperty =
            DependencyProperty.Register("Arguments", typeof(ExpressionCollection), typeof(Call), null);

        public string MethodName { get; set; }

        public string StaticMethodName { get; set; }

        public ExpressionCollection TypeArguments
        {
            get { return (ExpressionCollection)GetValue(TypeArgumentsProperty); }
            set { SetValue(TypeArgumentsProperty, value); }
        }

        public static readonly DependencyProperty TypeArgumentsProperty =
            DependencyProperty.Register("TypeArguments", typeof(ExpressionCollection), typeof(Call), null);

        public string FunctionName { get; set; }

        public BuiltinFunction BuiltinFunction { get; set; }

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(ArgumentsProperty, TypeArgumentsProperty);
        }

        protected override object OnEvaluate(Engine engine)
        {
            var args = Arguments.Evaluate(engine);
            if (engine.HasBindingOrValue(ParameterProperty, ParameterPath))
            {
                var parameter = engine.Evaluate(ParameterProperty, ParameterPath);
                args = new object[] { engine.EvaluateObject(parameter) }.Concat(args).ToArray();
            }
            return CallHelper.Call(Path, StaticMethodName, MethodName, FunctionName, BuiltinFunction,
                engine.EvaluateType(TypeProperty, TypeName), TypeArguments, args, engine);
        }
    }
}
