#if !INTERACTIVITY

namespace Markup.Programming.Core
{
    public abstract class PassiveAttachableComponent : AttachableComponent
    {
        protected abstract void Invoke(object parameter);
    }
}

#endif
