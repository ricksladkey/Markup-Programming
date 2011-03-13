#if !INTERACTIVITY

using System.Windows;

namespace Markup.Programming.Core
{
    public class ComponentBase : BindingCapableObject, IComponent
    {
        private DependencyObject associatedObject;

        public void Attach(DependencyObject dependencyObject)
        {
            associatedObject = dependencyObject;
            OnAttached();
        }

        public void Detach()
        {
            OnDetaching();
            associatedObject = null;
        }

        public DependencyObject AssociatedObject
        {
            get { return associatedObject; }
        }

        protected virtual void OnAttached()
        {
        }

        protected virtual void OnDetaching()
        {
        }
    }
}

#endif
