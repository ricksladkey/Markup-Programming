using System.Collections.Generic;
namespace Markup.Programming.Core
{
    public class IfNode : StatementNode
    {
        public struct Pair
        {
            public ExpressionNode Expression { get; set; }
            public StatementNode Statement { get; set; }
        };
        public IList<Pair> Pairs { get; set; }
        public StatementNode Else { get; set; }
        protected override void OnExecute(Engine engine)
        {
            foreach (var pair in Pairs)
            {
                if (TypeHelper.ConvertToBool(pair.Expression.Get(engine)))
                {
                    pair.Statement.Execute(engine);
                    return;
                }
                if (engine.ShouldInterrupt) return;
            }
            if (Else != null) Else.Execute(engine);
        }
    }
}
