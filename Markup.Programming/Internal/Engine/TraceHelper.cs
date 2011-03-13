using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Markup.Programming.Core
{
    /// <summary>
    /// The TraceHelper class is used to produce trace diagnostics.
    /// </summary>
    public static class TraceHelper
    {
        [Conditional("TRACE")]
        public static void WriteLine(string format, params object[] parameters)
        {
#if !SILVERLIGHT
            Trace.WriteLine(string.Format(format, parameters));
#else
            Debug.WriteLine(string.Format(format, parameters));
#endif
        }
    }
}
