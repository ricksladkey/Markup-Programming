using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;

namespace Markup.Programming.Core
{
    /// <summary>
    /// An HandlerCollection is a collection of Handler items.
    /// </summary>
    public class HandlerCollection : ComponentCollection<Handler>
    {
        private bool topLevelCollection;

        internal void AttachOperations(DependencyObject dependencyObject)
        {
            topLevelCollection = true;
            Attach(dependencyObject);
        }

        protected override void OnAttached(IEnumerable<IComponent> components)
        {
            if (topLevelCollection) SetTopLevel(components);
            base.OnAttached(components);
        }

        private void SetTopLevel(IEnumerable<IComponent> components)
        {
            foreach (Handler operation in this) operation.TopLevelOperation = true;
        }
    }
}
