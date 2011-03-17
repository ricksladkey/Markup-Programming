using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;

namespace Markup.Programming.Core
{
    /// <summary>
    /// The Configuration class allows us to test the
    /// Silverlight configuration on the desktop.
    /// </summary>
    public static class Configuration
    {

#if !FULL_TRACING
        public static TraceFlags TraceDefaults { get { return TraceFlags.Console; } }
#else
        public static TraceFlags TraceDefaults { get { return TraceFlags.All; } }
#endif

#if !SILVERLIGHT && !EMULATE_SILVERLIGHT
        public static bool Silverlight { get { return false; } }
#else
        public static bool Silverlight { get { return true; } }
#endif

        public static bool IsInDesignMode
        {
            get
            {
#if !SILVERLIGHT
                if (!Silverlight)
                {
                    var property = DesignerProperties.IsInDesignModeProperty;
                    return (bool)DependencyPropertyDescriptor.FromProperty(property, typeof(FrameworkElement)).Metadata.DefaultValue;
                }
#endif
                return false;
            }
        }
    }
}
