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
            foreach (var statement in this)
            {
                statement.Execute(engine);
                if (engine.ShouldInterrupt) break;
            }
        }

        public void ExecuteSkipOne(Engine engine)
        {
            foreach (var statement in this.Skip(1))
            {
                statement.Execute(engine);
                if (engine.ShouldInterrupt) break;
            }
        }
    }
}
