using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;

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
            if (e.NewItems == null) return;
            if (this.AssociatedObject == null) return;
            var attach = new List<IComponent>();
            var detach = new List<IComponent>();
            foreach (T item in e.NewItems)
            {
                if (item.AssociatedObject == null) { attach.Add(item); continue; }
                if (item.AssociatedObject != this.AssociatedObject) { detach.Add(item); continue; }
            }
            OnAttached(attach);
            OnDetaching(detach);
        }

        private DependencyObject associatedObject;

        public DependencyObject AssociatedObject
        {
            get { return associatedObject; }
        }

        public void Attach(DependencyObject depedencyObject)
        {
            associatedObject = depedencyObject;
            OnAttached(this.Select(item => item as IComponent));
        }

        public void Detach()
        {
            OnDetaching(this.Select(item => item as IComponent));
            associatedObject = null;
        }


        protected virtual void OnAttached(IEnumerable<IComponent> components)
        {
            foreach (var item in components) { item.Detach(); item.Attach(associatedObject); }
        }

        protected virtual void OnDetaching(IEnumerable<IComponent> components)
        {
            foreach (var item in components) item.Detach();
        }
    }
}
