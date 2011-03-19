﻿using System;
using System.Linq;
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
    public class CallHandler : Handler
    {
        public CallHandler()
        {
            Arguments = new ExpressionCollection();
            TypeArguments = new ExpressionCollection();
        }

        public Type Type
        {
            get { return (Type)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(Type), typeof(CallHandler), null);

        public string TypeName { get; set; }

        public object Argument
        {
            get { return (object)GetValue(ArgumentProperty); }
            set { SetValue(ArgumentProperty, value); }
        }

        public static readonly DependencyProperty ArgumentProperty =
            DependencyProperty.Register("Argument", typeof(object), typeof(CallHandler), null);

        public string ArgumentPath { get; set; }

        private PathExpression argumentPathExpression = new PathExpression();
        protected PathExpression ArgumentPathExpression { get { return argumentPathExpression; } }

        public ExpressionCollection Arguments
        {
            get { return (ExpressionCollection)GetValue(ArgumentsProperty); }
            set { SetValue(ArgumentsProperty, value); }
        }

        public static readonly DependencyProperty ArgumentsProperty =
            DependencyProperty.Register("Arguments", typeof(ExpressionCollection), typeof(CallHandler), null);

        public string MethodName { get; set; }

        public string StaticMethodName { get; set; }

        public ExpressionCollection TypeArguments
        {
            get { return (ExpressionCollection)GetValue(TypeArgumentsProperty); }
            set { SetValue(TypeArgumentsProperty, value); }
        }

        public static readonly DependencyProperty TypeArgumentsProperty =
            DependencyProperty.Register("TypeArguments", typeof(ExpressionCollection), typeof(CallHandler), null);

        public string FunctionName { get; set; }

        public BuiltinFunction BuiltinFunction { get; set; }

        public string PathEventName { get { return GetFields()[0]; } }

        public string PathBase { get { return GetFields()[1]; } }

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(ArgumentsProperty, TypeArgumentsProperty);
        }

        private string[] GetFields()
        {
            int m = Path.IndexOf('.');
            if (m == -1) ThrowHelper.Throw("missing event");
            return new string[] { Path.Substring(0, m), Path.Substring(m + 1) };
        }

        protected override void OnActiveExecute(Engine engine)
        {
            RegisterHandler(engine, PathEventName);
        }

        protected override void OnEventHandler(Engine engine)
        {
            var args = Arguments.Evaluate(engine);
            if (engine.HasBindingOrValue(ArgumentProperty, ArgumentPath))
            {
                var parameter = engine.Evaluate(ArgumentProperty, ArgumentPath, ArgumentPathExpression);
                args = new object[] { engine.EvaluateObject(parameter) }.Concat(args).ToArray();
            }
            CallHelper.Call(PathBase, PathExpression, StaticMethodName, MethodName, FunctionName, BuiltinFunction,
                engine.EvaluateType(TypeProperty, TypeName), TypeArguments, args, engine);
        }
    }
}