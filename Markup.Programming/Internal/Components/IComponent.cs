namespace Markup.Programming.Core
{
#if !INTERACTIVITY
    using System.Windows;
    /// <summary>
    /// An IComponent is an object that can be attached to
    /// another dependency object.
    /// </summary>
    public interface IComponent
    {
        void Attach(DependencyObject dependencyObject);
        void Detach();

        DependencyObject AssociatedObject { get; }
    }
#else
    using System.Windows.Interactivity; // portable
    /// <summary>
    /// An IComponent is an object that can be attached to
    /// another dependency object.
    /// </summary>
    public interface IComponent : IAttachedObject
    {
    }
#endif
}
