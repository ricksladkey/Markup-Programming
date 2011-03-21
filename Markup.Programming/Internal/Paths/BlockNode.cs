using System.Collections.Generic;

namespace Markup.Programming.Core
{
    public class BlockNode : StatementNode
    {
        public IList<StatementNode> Nodes { get; set; }
        protected override void OnExecute(Engine engine)
        {
            foreach (var node in Nodes)
            {
                node.Execute(engine);
                if (engine.ShouldInterrupt) break;
            }
        }
    }
}
