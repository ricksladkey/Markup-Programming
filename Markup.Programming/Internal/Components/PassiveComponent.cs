
namespace Markup.Programming.Core
{
#if !INTERACTIVITY
    public abstract class PassiveComponent : PassiveAttachableComponent
    {
    }
#else
    using System.Windows;
    using System.Windows.Interactivity; // portable

    public abstract class PassiveComponent : TriggerAction<DependencyObject>, IComponent
    {
    }
#endif
}
