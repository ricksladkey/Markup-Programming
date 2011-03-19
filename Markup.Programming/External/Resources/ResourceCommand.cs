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
    public class ResourceCommand : InterfaceComponent, ICommand
    {
        private CommandInterop<ResourceCommand> interop;

        public ResourceCommand()
        {
            interop = new CommandInterop<ResourceCommand>(this);
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
