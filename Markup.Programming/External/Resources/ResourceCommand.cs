using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// A ResourceCommand is an ICommand that can be defined and
    /// referenced entirely in resources and by IExpression
    /// and IStatement to implement the CanExecute and Execute
    /// interface methods.
    /// </summary>
    [ContentProperty("Functions")]
    public class ResourceCommand : ResourceComponent, ICommand, IInteropHost
    {
        private CommandInterop<ResourceCommand> interop;

        public ResourceCommand()
        {
            Functions = new FunctionCollection();
            interop = new CommandInterop<ResourceCommand>(this);
        }

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
            engine.SetContext(null);
            Functions.Execute(engine);
            return engine.CallFunction(function, args);
        }

#if SILVERLIGHT
        public event System.EventHandler CanExecuteChanged;
        private void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null) CanExecuteChanged(this, new EventArgs());
        }
#else
        public event System.EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
#endif

        public bool CanExecute(object parameter)
        {
            TryToAttach();
            return interop.CanExecute(parameter);
        }

        public void Execute(object parameter)
        {
            TryToAttach();
            interop.Execute(parameter);
        }
    }
}
