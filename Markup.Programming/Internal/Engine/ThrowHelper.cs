using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;

namespace Markup.Programming.Core
{
    public class ThrowHelper
    {
        public static object Throw(string message)
        {
            return Throw(new InvalidOperationException(message));
        }

        public static object Throw(Exception exception)
        {
#if DEBUG
            if (Debugger.IsAttached) Debugger.Break();
            if (Application.Current == null) throw exception;
            string message = exception.Message;
            MessageBox.Show(message);
#endif
            throw exception;
        }
    }
}
