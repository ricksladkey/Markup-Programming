namespace Markup.Programming.Core
{
    public class SetNode : ExpressionNode
    {
        public ExpressionNode LValue { get; set; }
        public ExpressionNode RValue { get; set; }
        public AssignmentOp Op { get; set; }
        protected override object OnGet(Engine engine)
        {
            if (Op == AssignmentOp.Assign) return LValue.Set(engine, RValue.Get(engine));
            return LValue.Set(engine, engine.Operator(Op, LValue.Get(engine), RValue.Get(engine)));
        }
    }
}
