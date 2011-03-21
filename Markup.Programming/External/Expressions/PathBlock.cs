using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using Markup.Programming.Core;

namespace Markup.Programming
{
    [ContentProperty("Body")]
    public class PathBlock : ExpressionBase
    {
        public string Body { get; set; }

        protected override object OnEvaluate(Engine engine)
        {
            engine.SetReturnFrame();
            if (Body == null) return null;
            engine.ExecutePathBlock(Body, PathExpression);
            return engine.GetAndResetReturnValue();
        }
    }
}
