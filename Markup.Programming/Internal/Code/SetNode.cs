namespace Markup.Programming.Core
{
    public class SetNode : ExpressionNode
    {
        public ExpressionNode LValue { get; set; }
        public ExpressionNode RValue { get; set; }
        public AssignmentOp Op { get; set; }
        protected override object OnEvaluate(Engine engine)
        {
            if (Op == AssignmentOp.Assign) return LValue.Set(engine, RValue.Evaluate(engine));
            return LValue.Set(engine, engine.Evaluate(Op, LValue.Evaluate(engine), RValue.Evaluate(engine)));
        }
    }
}
