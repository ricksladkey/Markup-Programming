using System;
using System.Windows;
using System.Windows.Markup;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// The Call expression calls a method or a function.
    /// </summary>
    [ContentProperty("Arguments")]
    public class Call : ExpressionBase, ICaller
    {
        private CallImplementor<Call> implementor;

        public Call()
        {
            implementor = new CallImplementor<Call>(this);
            Arguments = new ExpressionCollection();
            TypeArguments = new ExpressionCollection();
        }

        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(Call), null);

        public Type Type
        {
            get { return (Type)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(Type), typeof(Call), null);

        public string TypeName
        {
            get { return (string)GetValue(TypeNameProperty); }
            set { SetValue(TypeNameProperty, value); }
        }

        public static readonly DependencyProperty TypeNameProperty =
            DependencyProperty.Register("TypeName", typeof(string), typeof(Call), null);

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

        public string MethodName
        {
            get { return (string)GetValue(MethodNameProperty); }
            set { SetValue(MethodNameProperty, value); }
        }

        public static readonly DependencyProperty MethodNameProperty =
            DependencyProperty.Register("MethodName", typeof(string), typeof(Call), null);

        public string StaticMethodName
        {
            get { return (string)GetValue(StaticMethodNameProperty); }
            set { SetValue(StaticMethodNameProperty, value); }
        }

        public static readonly DependencyProperty StaticMethodNameProperty =
            DependencyProperty.Register("StaticMethodName", typeof(string), typeof(Call), null);

        public ExpressionCollection TypeArguments
        {
            get { return (ExpressionCollection)GetValue(TypeArgumentsProperty); }
            set { SetValue(TypeArgumentsProperty, value); }
        }

        public static readonly DependencyProperty TypeArgumentsProperty =
            DependencyProperty.Register("TypeArguments", typeof(ExpressionCollection), typeof(Call), null);

        public string FunctionName
        {
            get { return (string)GetValue(FunctionNameProperty); }
            set { SetValue(FunctionNameProperty, value); }
        }

        public static readonly DependencyProperty FunctionNameProperty =
            DependencyProperty.Register("FunctionName", typeof(string), typeof(Call), null);


        public BuiltinFunction BuiltinFunction
        {
            get { return (BuiltinFunction)GetValue(BuiltinFunctionProperty); }
            set { SetValue(BuiltinFunctionProperty, value); }
        }

        public static readonly DependencyProperty BuiltinFunctionProperty =
            DependencyProperty.Register("BuiltinFunction", typeof(BuiltinFunction), typeof(Call), null);

        public string PathBase { get { return Path; } }

        public DependencyProperty CallerTypeProperty { get { return TypeProperty; } }

        public DependencyProperty CallerParameterProperty { get { return ParameterProperty; } }

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(ArgumentsProperty, TypeArgumentsProperty);
        }

        protected override object OnEvaluate(Engine engine)
        {
            return implementor.Call(engine);
        }
    }
}
