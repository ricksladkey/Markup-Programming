namespace Markup.Programming.Core
{
    public interface IInteropHost
    {
        object Callback(object child, string function, object[] args, Engine engine);
    }
}
