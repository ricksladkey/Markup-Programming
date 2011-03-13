using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// An AttachedHandler is a handler that raises only once when it is
    /// attached to its associated object.  This is in general before the
    /// associated object is loaded.
    /// </summary>
    public class AttachedHandler : Handler
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            ExecuteBody(null, null);
        }
    }
}
