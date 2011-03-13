namespace Markup.Programming.Core
{
#if !INTERACTIVITY
    public abstract class ActiveComponent : ActiveAttachableComponent
    {
        protected void InvokePassiveComponents(object sender, object e)
        {
            ExecuteBody(sender, e);
        }
    }
#else
    using System.Windows;
    using System.Windows.Interactivity; // portable
    public class ActiveComponent : TriggerBase<DependencyObject>, IComponent
    {
        protected void InvokePassiveComponents(object sender, object e)
        {
            InvokeActions(e);
        }
    }
#endif
}
