using System.Collections.Generic;

namespace Markup.Programming.Core
{
    public class BlockNode : PathNode
    {
        public IList<PathNode> Nodes { get; set; }
        protected override object OnEvaluate(Engine engine, object value)
        {
            Execute(engine);
            return null;
        }
        public void Execute(Engine engine)
        {
            OnExecute(engine);
        }

        protected virtual void OnExecute(Engine engine)
        {
            foreach (var node in Nodes) node.Evaluate(engine, UnsetValue.Value);
        }
    }
}
