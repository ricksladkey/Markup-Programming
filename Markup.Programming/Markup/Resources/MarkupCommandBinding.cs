using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.ComponentModel;

namespace Markup.Programming
{
    /// <summary>
    /// A MarkupCommandBinding creates a command binding that forwards routed
    /// command events to a handler command.
    /// </summary>
    public class MarkupCommandBinding : CommandBinding, ISupportInitialize
    {
        public ICommand HandlerCommand { get; set; }

        public void BeginInit()
        {
        }

        public void EndInit()
        {
            Executed += (s, e) => HandlerCommand.Execute(e.Parameter);
            CanExecute += (s, e) => e.CanExecute = HandlerCommand.CanExecute(e.Parameter);
        }
    }
}
