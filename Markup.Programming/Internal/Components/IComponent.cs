#if INTERACTIVITY
using System.Windows.Interactivity; // portable
#endif

namespace Markup.Programming.Core
{
    /// <summary>
    /// An IComponent is an object that can be attached to
    /// another dependency object.
    /// </summary>
    public interface IComponent : IAttachedObject
    {
    }
}
