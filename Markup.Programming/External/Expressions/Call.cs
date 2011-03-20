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

        public object Type
        {
            get { return (object)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(object), typeof(Call), null);

        public string TypePath { get; set; }

        private PathExpression typePathExpression = new PathExpression();
        protected PathExpression TypePathExpression { get { return typePathExpression; } }

        public object Argument
        {
            get { return (object)GetValue(ArgumentProperty); }
            set { SetValue(ArgumentProperty, value); }
        }

        public static readonly DependencyProperty ArgumentProperty =
            DependencyProperty.Register("Argument", typeof(object), typeof(Call), null);

        public string ArgumentPath { get; set; }

        private PathExpression argumentPathExpression = new PathExpression();
        protected PathExpression ArgumentPathExpression { get { return argumentPathExpression; } }

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
            Attach(TypeProperty, ArgumentsProperty, TypeArgumentsProperty);
        }

        protected override object OnEvaluate(Engine engine)
        {
            var args = Arguments.Evaluate(engine);
            if (engine.HasBindingOrValue(ArgumentProperty, ArgumentPath))
            {
                var parameter = engine.Evaluate(ArgumentProperty, ArgumentPath, ArgumentPathExpression);
                args = new object[] { engine.EvaluateObject(parameter) }.Concat(args).ToArray();
            }
            return CallHelper.Call(Path, PathExpression, StaticMethodName, MethodName, FunctionName, BuiltinFunction,
                engine.EvaluateType(TypeProperty, TypePath, TypePathExpression), TypeArguments, args, engine);
        }
    }
}
