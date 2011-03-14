using System;
using System.Windows;
using System.Windows.Markup;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// The CallHandler handler calls a method or a function when an
    /// event is raised.
    /// </summary>
    [ContentProperty("Arguments")]
    public class CallHandler : Handler, ICaller
    {
        private CallImplementor<CallHandler> implementor;

        public CallHandler()
        {
            implementor = new CallImplementor<CallHandler>(this);
            Arguments = new ExpressionCollection();
            TypeArguments = new ExpressionCollection();
        }

        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(CallHandler), null);

        public string ParameterPath { get; set; }

        public Type Type
        {
            get { return (Type)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(Type), typeof(CallHandler), null);

        public string TypeName
        {
            get { return (string)GetValue(TypeNameProperty); }
            set { SetValue(TypeNameProperty, value); }
        }

        public static readonly DependencyProperty TypeNameProperty =
            DependencyProperty.Register("TypeName", typeof(string), typeof(CallHandler), null);

        public object Parameter
        {
            get { return (object)GetValue(ParameterProperty); }
            set { SetValue(ParameterProperty, value); }
        }

        public static readonly DependencyProperty ParameterProperty =
            DependencyProperty.Register("Parameter", typeof(object), typeof(CallHandler), null);

        public ExpressionCollection Arguments
        {
            get { return (ExpressionCollection)GetValue(ArgumentsProperty); }
            set { SetValue(ArgumentsProperty, value); }
        }

        public static readonly DependencyProperty ArgumentsProperty =
            DependencyProperty.Register("Arguments", typeof(ExpressionCollection), typeof(CallHandler), null);

        public string MethodName
        {
            get { return (string)GetValue(MethodNameProperty); }
            set { SetValue(MethodNameProperty, value); }
        }

        public static readonly DependencyProperty MethodNameProperty =
            DependencyProperty.Register("MethodName", typeof(string), typeof(CallHandler), null);

        public string StaticMethodName
        {
            get { return (string)GetValue(StaticMethodNameProperty); }
            set { SetValue(StaticMethodNameProperty, value); }
        }

        public static readonly DependencyProperty StaticMethodNameProperty =
            DependencyProperty.Register("StaticMethodName", typeof(string), typeof(CallHandler), null);

        public ExpressionCollection TypeArguments
        {
            get { return (ExpressionCollection)GetValue(TypeArgumentsProperty); }
            set { SetValue(TypeArgumentsProperty, value); }
        }

        public static readonly DependencyProperty TypeArgumentsProperty =
            DependencyProperty.Register("TypeArguments", typeof(ExpressionCollection), typeof(CallHandler), null);

        public string FunctionName
        {
            get { return (string)GetValue(FunctionNameProperty); }
            set { SetValue(FunctionNameProperty, value); }
        }

        public static readonly DependencyProperty FunctionNameProperty =
            DependencyProperty.Register("FunctionName", typeof(string), typeof(CallHandler), null);


        public BuiltinFunction BuiltinFunction
        {
            get { return (BuiltinFunction)GetValue(BuiltinFunctionProperty); }
            set { SetValue(BuiltinFunctionProperty, value); }
        }

        public static readonly DependencyProperty BuiltinFunctionProperty =
            DependencyProperty.Register("BuiltinFunction", typeof(BuiltinFunction), typeof(CallHandler), null);

        public string PathEventName { get { return GetFields()[0]; } }

        public string PathBase { get { return GetFields()[1]; } }

        public DependencyProperty CallerTypeProperty { get { return TypeProperty; } }

        public DependencyProperty CallerParameterProperty { get { return ParameterProperty; } }

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(ArgumentsProperty, TypeArgumentsProperty);
            new Engine().With(this, engine => RegisterHandler(engine, PathEventName));
        }

        private string[] GetFields()
        {
            int m = Path.IndexOf('.');
            if (m == -1) ThrowHelper.Throw("missing event");
            return new string[] { Path.Substring(0, m), Path.Substring(m + 1) };
        }

        protected override void OnEventHandler(Engine engine)
        {
            implementor.Call(engine);
        }
    }
}
