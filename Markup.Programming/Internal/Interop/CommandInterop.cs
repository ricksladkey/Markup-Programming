using System;
using System.Windows.Input;

namespace Markup.Programming.Core
{
    public class CommandInterop : Interop, ICommand
    {
        public CommandInterop(IInteropHost parent) { Parent = parent; }

        public event System.EventHandler CanExecuteChanged;

        protected void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null) CanExecuteChanged(this, new EventArgs());
        }

        public bool CanExecute(object parameter)
        {
            return (bool)TypeHelper.ConvertToBool(Callback("CanExecute", parameter));
        }

        public void Execute(object parameter)
        {
            Callback("Execute", parameter);
        }
    }
}
