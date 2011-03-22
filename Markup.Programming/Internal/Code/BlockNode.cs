using System.Collections.Generic;

namespace Markup.Programming.Core
{
    public class BlockNode : FramedStatementNode
    {
        public IList<StatementNode> Nodes { get; set; }
        protected override void OnExecuteFrame(Engine engine)
        {
            foreach (var node in Nodes)
            {
                node.Execute(engine);
                if (engine.ShouldInterrupt) break;
            }
        }
    }
}
