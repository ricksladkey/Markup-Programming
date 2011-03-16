using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace Markup.Programming.Core
{
    public class ThrowHelper
    {
        public static object Throw(string message, params object[] args)
        {
            return Throw(new InvalidOperationException(string.Format(message, args)));
        }

        public static object Throw(Exception exception)
        {
            return Throw(exception, null);
        }

        public static object Throw(Exception exception, Engine engine)
        {
#if DEBUG
            if (Debugger.IsAttached) Debugger.Break();
            if (Application.Current == null || Configuration.IsInDesignMode) throw exception;
            string message = "Fatal error: " + exception.Message + "\n";
            if (engine != null)
            {
                message += engine.StackBackwards.Aggregate("", (s, frame) => s + "\n" + frame.CallerName) + "\n";
                message += "\nAssociated object: " + engine.StackBackwards.First().Caller.AssociatedObject;
            }
            MessageBox.Show(message);
#endif
            throw exception;
        }
    }
}
