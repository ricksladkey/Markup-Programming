using System.Collections.Generic;
using System.Linq;

namespace Markup.Programming.Core
{
    public class OpNode : PathNode
    {
        public OpNode() { Operands = new List<PathNode>(); }
        public Op Op { get; set; }
        public IList<PathNode> Operands { get; set; }
        protected override object OnEvaluate(Engine engine, object value)
        {
            return engine.Evaluate(Op, Operands.Select(operand => operand.Evaluate(engine, value)).ToArray());
        }
    }
}
