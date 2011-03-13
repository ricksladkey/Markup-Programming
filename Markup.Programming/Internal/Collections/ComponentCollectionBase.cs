using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Markup.Programming.Core
{

#if !SILVERLIGHT

    /// <summary>
    /// An ComponentCollectionBase is a freezable collection on WPF in order
    /// to support propagation of the inheritance context.
    /// </summary>
    /// <typeparam name="T">The type of the component item</typeparam>
    public class ComponentCollectionBase<T> : FreezableCollection<T> where T : DependencyObject
    {
    }

#else

    /// <summary>
    /// An ComponentCollectionBase is a collection of DependencyObject
    /// on Silverlight because Silverlight does not support Freezable.
    /// </summary>
    /// <typeparam name="T">The type of the component item</typeparam>
    public class ComponentCollectionBase<T> : DependencyObjectCollection<T> where T : DependencyObject
    {
    }

#endif

}
