namespace Markup.Programming.Core
{

    /// <summary>
    /// An IComponent is an object that can be attached to
    /// another dependency object.
    /// </summary>
#if !INTERACTIVITY
    using System.Windows;
    public interface IComponent
    {
        void Attach(DependencyObject dependencyObject);
        void Detach();

        DependencyObject AssociatedObject { get; }
    }
#else
    using System.Windows.Interactivity; // portable
    public interface IComponent : IAttachedObject
    {
    }
#endif
}
