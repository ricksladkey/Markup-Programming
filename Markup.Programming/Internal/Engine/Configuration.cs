using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming.Core
{
    /// <summary>
    /// The Configuration class allows us to test the
    /// Silverlight configuration on the desktop.
    /// </summary>
    internal static class Configuration
    {

#if !FULL_TRACING
        public static TraceFlags TraceDefaults = TraceFlags.Console;
#else
        public static TraceFlags TraceDefaults = TraceFlags.All;
#endif

#if !SILVERLIGHT && !EMULATE_SILVERLIGHT
        public static bool Silverlight = false;
#else
        public static bool Silverlight = true;
#endif

    }
}
