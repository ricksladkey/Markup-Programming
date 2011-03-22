using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming.Core
{
    public class IncrementNode : ExpressionNode
    {
        public ExpressionNode LValue { get; set; }
        public bool PostFix { get; set; }
        public int Increment { get; set; }
        protected override object OnEvaluate(Engine engine)
        {
            var oldValue = (int)TypeHelper.Convert(LValue.Evaluate(engine), typeof(int));
            LValue.Set(engine, oldValue + Increment);
            return PostFix ? oldValue : oldValue + Increment;
        }
    }
}
