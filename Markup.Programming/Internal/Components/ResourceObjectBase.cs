using System;
using System.Windows;
using System.Reflection;
using System.ComponentModel;

namespace Markup.Programming.Core
{
    /// <summary>
    /// An ResourceObjectBase is an object that implements IComponent but
    /// but doesn't otherwise derive from any component classes.
    /// This is used as the base class for classes like MarkupObject,
    /// ResourceCommand, etc. that are intended to be defined in XAML resources.
    /// </summary>
    public abstract class ResourceObjectBase : BindingCapableObject, ISupportInitialize, IValueProvider, IComponent
    {
        private DependencyObject associatedObject;

        public DependencyObject AssociatedObject
        {
            get { return associatedObject; }
        }

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

        protected virtual void OnAttached()
        {
        }

        protected virtual void OnDetaching()
        {
        }

#if !SILVERLIGHT
        private static PropertyInfo inheritanceContextPropertyInfo = typeof(Freezable).GetProperty("InheritanceContext", BindingFlags.Instance | BindingFlags.NonPublic);

        private void TryToGetInheritanceContext()
        {
            if (AssociatedObject == null)
            {
                var inheritanceContext = inheritanceContextPropertyInfo.GetValue(this, null) as DependencyObject;
                if (inheritanceContext is FrameworkElement)
                    Attach(inheritanceContext);
            }
        }
#endif

        protected virtual void TryToAttach()
        {
#if !SILVERLIGHT
            if (!Configuration.Silverlight) TryToGetInheritanceContext();
#endif
        }

        public virtual void BeginInit()
        {
        }

        public virtual void EndInit()
        {
            TryToAttach();
        }

        public virtual object Value
        {
            get { return this; }
            set { ThrowHelper.Throw(new NotImplementedException()); }
        }

        protected void Attach(params DependencyProperty[] properties)
        {
            ExecutionHelper.Attach(this, properties);
        }
    }
}
