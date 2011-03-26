namespace Markup.Programming.Core
{
    public abstract class LazyResourceComponent : ResourceComponent
    {
        protected bool IsInitialized { get; private set; }

        protected void Initialize()
        {
            if (IsInitialized) return;
            IsInitialized = true;
            new Engine().FrameAction(this, OnInitialize);
        }

        protected abstract void OnInitialize(Engine engine);
    }
}
