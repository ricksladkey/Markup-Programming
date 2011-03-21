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
            if (Body == null) return null;
            return engine.GetPathBlock(Body, PathExpression);
        }
    }
}
