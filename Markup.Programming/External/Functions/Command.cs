using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Markup.Programming.Core;

namespace Markup.Programming
{
    [ContentProperty("Functions")]
    public class Command : HiddenExpression, IInteropHost
    {
        public Command()
        {
            Functions = new FunctionCollection();
        }

        public ResourceComponent ParentResourceObject { get; set; }

        public FunctionCollection Functions
        {
            get { return (FunctionCollection)GetValue(FunctionsProperty); }
            set { SetValue(FunctionsProperty, value); }
        }

        public static readonly DependencyProperty FunctionsProperty =
            DependencyProperty.Register("Functions", typeof(FunctionCollection), typeof(Command), null);

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(FunctionsProperty);
        }

        public object Callback(object child, string function, object[] args, Engine engine)
        {
            return engine.EvaluateFrame(this, e => CallFunction(child, function, args, engine));
        }

        private object CallFunction(object child, string function, object[] args, Engine engine)
        {
            engine.SetContext(ParentResourceObject.Value);
            Functions.Execute(engine);
            return engine.CallFunction(function, args);
        }

        protected override object OnEvaluate(Engine engine)
        {
            ParentResourceObject = engine.ParentResourceObject;
            return new CommandInterop(this);
        }
    }
}
