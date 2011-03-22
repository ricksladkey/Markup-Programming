using System;
namespace Markup.Programming.Core
{
    public class IteratorNode : FramedExpressionNode
    {
        public ExpressionNode Type { get; set; }
        public StatementNode Body { get; set; }
        protected override object  OnEvaluateFrame(Engine engine)
        {
            engine.SetYieldFrame();
            var type = Type != null ? Type.Evaluate(engine) as Type : null;
            Body.Execute(engine);
            return TypeHelper.CreateCollection(engine.GetYieldedValues(), type, null);
        }
    }
}
