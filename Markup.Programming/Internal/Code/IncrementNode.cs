using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming.Core
{
    public class IncrementNode : ExpressionNode
    {
        public ExpressionNode LValue { get; set; }
        public AssignmentOp Op { get; set; }
        private int Increment { get { return Op == AssignmentOp.Increment || Op == AssignmentOp.PostIncrement ? 1 : -1; } }
        private bool IsPostfix { get { return Op == AssignmentOp.PostIncrement || Op == AssignmentOp.PostDecrement; } }
        protected override object OnGet(Engine engine)
        {
            var oldValue = (int)TypeHelper.Convert(LValue.Get(engine), typeof(int));
            LValue.Set(engine, oldValue + Increment);
            return IsPostfix ? oldValue : oldValue + Increment;
        }
    }
}
