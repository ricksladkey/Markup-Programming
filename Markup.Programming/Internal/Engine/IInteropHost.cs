namespace Markup.Programming.Core
{
    public interface IInteropHost
    {
        object Interop(object child, string function, object[] args, Engine engine);
    }
}
