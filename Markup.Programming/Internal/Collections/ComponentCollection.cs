using System.Windows;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Markup.Programming.Core
{
    /// <summary>
    /// An ComponentCollection is a collection of DependencyObjects that
    /// implement IComponent that can be attached and detached as
    /// a group.  The collection itself is also an IComponent which
    /// it forwards to the items it contains.
    /// </summary>
    /// <typeparam name="T">The type of the component object</typeparam>
#if DEBUG
    [DebuggerDisplay("Components = {Count}"), DebuggerTypeProxy(typeof(CollectionDebugView))]
#endif
    public class ComponentCollection<T> : ComponentCollectionBase<T>, IComponent where T : DependencyObject, IComponent
    {
        public ComponentCollection()
        {
            (this as INotifyCollectionChanged).CollectionChanged += OnCollectionChanged;
        }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (IComponent item in e.NewItems)
            {
                if (this.AssociatedObject == null) return;
                if (item.AssociatedObject == null)
                {
                    item.Attach(this.AssociatedObject);
                    continue;
                }
                if (item.AssociatedObject != this.AssociatedObject)
                {
                    item.Detach();
                    item.Attach(this.AssociatedObject);
                    continue;
                }
            }
        }

        private DependencyObject associatedObject;

        public DependencyObject AssociatedObject
        {
            get { return associatedObject; }
        }

        public void Attach(DependencyObject depedencyObject)
        {
            associatedObject = depedencyObject;
            foreach (var item in this) item.Attach(associatedObject);
            OnAttached();
        }

        public void Detach()
        {
            OnDetaching();
            foreach (var item in this) item.Detach();
            associatedObject = null;
        }

        protected virtual void OnAttached()
        {
        }

        protected virtual void OnDetaching()
        {
        }
    }
}
