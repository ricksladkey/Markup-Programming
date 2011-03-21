using System.Collections.Generic;

namespace Markup.Programming.Core
{
    public class BlockNode : PathNode
    {
        public IList<PathNode> Nodes { get; set; }
        protected override object OnEvaluate(Engine engine, object value)
        {
            foreach (var node in Nodes) node.Evaluate(engine, value);
            return null;
        }
    }
}
