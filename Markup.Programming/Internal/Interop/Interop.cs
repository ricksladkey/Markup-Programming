namespace Markup.Programming.Core
{
    public class Interop
    {
        public IInteropHost Parent { get; protected set; }

        protected object Callback(string method, params object[] args)
        {
            return Parent.Callback(this, "$" + method, args, new Engine());
        }
    }
}
