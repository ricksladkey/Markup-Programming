#if !INTERACTIVITY

using System.Windows;

namespace Markup.Programming.Core
{
    public interface IAttachedObject
    {
        void Attach(DependencyObject dependencyObject);
        void Detach();

        DependencyObject AssociatedObject { get; }
    }

}

#endif
