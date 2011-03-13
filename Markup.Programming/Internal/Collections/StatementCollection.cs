
using System.Diagnostics;
namespace Markup.Programming.Core
{
    /// <summary>
    /// An StatementCollection contains statements.
    /// This includes any kind of statement and in particular
    /// all statements and expressions are also actions.
    /// </summary>
#if DEBUG
    [DebuggerDisplay("Statements = {Count}"), DebuggerTypeProxy(typeof(CollectionDebugView))]
#endif
    public class StatementCollection : StatementCollectionBase<Statement>
    {
    }
}
