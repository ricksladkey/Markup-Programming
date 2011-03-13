using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Markup.Programming.Core
{

#if !SILVERLIGHT

    /// <summary>
    /// An BindingCapableObject object is a Freezable on WPF to support
    /// propagation of the inheritance context.
    /// </summary>
    public class BindingCapableObject : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            throw new NotImplementedException();
        }
    }

#else

    /// <summary>
    /// An ResourceObjectBase object is a DependencyObject on Silverlight
    /// because Silverlight does not support Freezable.
    /// </summary>
    public class ResourceObjectBase : DependencyObject
    {
    }

#endif

}
