using System;
using System.Windows.Input;

namespace Markup.Programming.Core
{
    public class CommandInterop<T> : Interop<T>, ICommand where T : IInteropHost
    {
        public CommandInterop(T parent) { Parent = parent; }

        public event System.EventHandler CanExecuteChanged;

        protected void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null) CanExecuteChanged(this, new EventArgs());
        }

        public bool CanExecute(object parameter)
        {
            var engine = new Engine();
            var rawResult = Parent.Callback(this, "$CanExecute", new object[] { parameter }, engine);
            var result = (bool)TypeHelper.Convert(rawResult, typeof(bool));
            return result;
        }

        public void Execute(object parameter)
        {
            var engine = new Engine();
            Parent.Callback(this, "$Execute", new object[] { parameter }, engine);
        }
    }
}
