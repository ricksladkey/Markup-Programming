using System.Collections.Generic;
using System.Windows;

namespace Markup.Programming.Core
{
    /// <summary>
    /// An HandlerCollection is a collection of Handler items.
    /// </summary>
    public class HandlerCollection : ComponentCollection<Handler>
    {
        private IDictionary<string, object> state;

        internal void AttachOperations(DependencyObject dependencyObject)
        {
            state = new Dictionary<string, object>();
            Attach(dependencyObject);
        }

        protected override void OnAttached(IEnumerable<IComponent> components)
        {
            foreach (Handler handler in components) handler.State = state;
            base.OnAttached(components);
        }
    }
}
