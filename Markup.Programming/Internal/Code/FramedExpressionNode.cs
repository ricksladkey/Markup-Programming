﻿namespace Markup.Programming.Core
{
    public abstract class FramedExpressionNode : ExpressionNode
    {
        public StatementNode Body { get; set; }
        protected override object OnGet(Engine engine)
        {
            return engine.EvaluateFrame(this, OnEvaluateFrame);
        }
        protected abstract object OnEvaluateFrame(Engine engine);
    }
}
