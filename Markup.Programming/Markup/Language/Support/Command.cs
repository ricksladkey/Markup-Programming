using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Markup.Programming.Core;
using System.Windows.Input;
using System.Windows;
using System.Windows.Markup;

namespace Markup.Programming
{
    [ContentProperty("Functions")]
    public class Command : HiddenExpression
    {
        public class CommandInterop : ICommand
        {
            public Command Parent { get; set; }

            public bool CanExecute(object parameter)
            {
                var engine = new Engine();
                var rawResult = Parent.Interop(this, "CanExecute", new object[] { parameter }, engine);
                var result = (bool)TypeHelper.Convert(rawResult, typeof(bool));
                return result;
            }

            public event System.EventHandler CanExecuteChanged;

            protected void OnCanExecuteChanged()
            {
                if (CanExecuteChanged != null) CanExecuteChanged(this, new EventArgs());
            }

            public void Execute(object parameter)
            {
                var engine = new Engine();
                Parent.Interop(this, "Execute", new object[] { parameter }, engine);
            }
        }

        public Command()
        {
            Functions = new FunctionCollection();
        }

        public ResourceObject ParentResourceObject { get; set; }

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

        public object Interop(object child, string function, object[] args, Engine engine)
        {
            return engine.With(this, e => CallFunction(child, function, args, engine));
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
            return new CommandInterop { Parent = this };
        }
    }
}
