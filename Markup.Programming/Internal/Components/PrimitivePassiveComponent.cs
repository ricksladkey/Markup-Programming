
namespace Markup.Programming.Core
{
#if !INTERACTIVITY
    public abstract class PrimitivePassiveComponent : AttachableComponent
    {
    }
#else
    using System.Windows;
    using System.Windows.Interactivity; // portable

    public abstract class PrimitivePassiveComponent : TriggerAction<DependencyObject>, IComponent
    {
        protected override void Invoke(object parameter)
        {
            if (this is IStatement) (this as IStatement).Execute(new Engine(parameter));
        }
    }
#endif
}
