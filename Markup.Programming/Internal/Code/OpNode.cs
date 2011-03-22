using System.Collections.Generic;
using System.Linq;

namespace Markup.Programming.Core
{
    public class OpNode : ExpressionNode
    {
        public OpNode() { Operands = new List<ExpressionNode>(); }
        public Op Op { get; set; }
        public IList<ExpressionNode> Operands { get; set; }
        protected override object OnEvaluate(Engine engine)
        {
            return engine.Evaluate(Op, Operands.Select(operand => operand.Evaluate(engine)).ToArray());
        }
    }
}
