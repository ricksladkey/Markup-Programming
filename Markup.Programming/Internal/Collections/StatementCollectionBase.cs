using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Markup.Programming.Core
{
    /// <summary>
    /// An StatementCollectionBase adds the capability to execute all of
    /// the actions in the collection.  A statement is executed,
    /// an expression is evaluated and an ordinary statement is invoked.
    /// 
    /// For complicated reasons we must inherit from a particular
    /// collection and derive elements from another class an this
    /// causes various forms of trouble.
    /// </summary>
    /// <typeparam name="T">The type of statement</typeparam>
    public abstract class StatementCollectionBase<T> : ComponentCollection<T> where T : Statement
    {
        public void Execute(Engine engine)
        {
            // Let the compiler choose whichever is more efficient.
            Execute(this, engine);
        }

        public void ExecuteSkipOne(Engine engine)
        {
            // Let the compiler choose whichever is more efficient.
            Execute(this.Skip(1), engine);
        }

        private static void Execute(IEnumerable<IStatement> statements, Engine engine)
        {
            engine.Execute(statements);
        }

        private static void Execute(IEnumerable<T> statements, Engine engine)
        {
            engine.Execute(statements.Select(statement => statement as IStatement));
        }
    }
}
