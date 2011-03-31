using System;
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

        public object Type
        {
            get { return (object)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(object), typeof(CallHandler), null);

        public string TypePath { get; set; }

        private CodeTree typeCodeTree = new CodeTree();
        protected CodeTree TypeCodeTree { get { return typeCodeTree; } }

        public object Argument
        {
            get { return (object)GetValue(ArgumentProperty); }
            set { SetValue(ArgumentProperty, value); }
        }

        public static readonly DependencyProperty ArgumentProperty =
            DependencyProperty.Register("Argument", typeof(object), typeof(CallHandler), null);

        public string ArgumentPath { get; set; }

        private CodeTree argumentCodeTree = new CodeTree();
        protected CodeTree ArgumentCodeTree { get { return argumentCodeTree; } }

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

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(TypeProperty, ArgumentsProperty, TypeArgumentsProperty);
        }

        protected override void OnActiveExecute(Engine engine)
        {
            if (Path != null)
            {
                CodeTree.Compile(engine, CodeType.Event | CodeType.Call, Path);
                RegisterHandler(engine, CodeTree.GetContext(engine), CodeTree.GetEvent(engine));
            }
            else
                RegisterHandler(engine, null, null);
        }

        protected override void OnEventHandler(Engine engine)
        {
            var args = Arguments.Get(engine);
            if (engine.HasBindingOrValue(ArgumentProperty, ArgumentPath))
            {
                var parameter = engine.Get(ArgumentProperty, ArgumentPath, ArgumentCodeTree);
                args = new object[] { engine.GetExpression(parameter) }.Concat(args).ToArray();
            }
            if (Path != null) CodeTree.Call(engine, args);
            else CallHelper.Call(Path, CodeTree, StaticMethodName, MethodName, FunctionName, BuiltinFunction,
                engine.GetType(TypeProperty, TypePath, TypeCodeTree), TypeArguments, args, engine);
        }
    }
}
