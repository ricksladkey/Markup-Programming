using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Markup.Programming.Core;

namespace Markup.Programming
{
    [ContentProperty("Script")]
    public class Command : HiddenExpression, IInteropHost
    {
        public Command()
        {
            Functions = new FunctionCollection();
        }

        public ResourceComponent ParentResourceObject { get; set; }

        public string Script { get; set; }

        private CodeTree codeTree = new CodeTree();
        protected CodeTree CodeTree { get { return codeTree; } }

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
            if (Script != null) engine.ExecuteScript(Script, CodeTree);
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
