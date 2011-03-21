using System.Windows.Markup;
using Markup.Programming.Core;

namespace Markup.Programming
{
    [ContentProperty("Expr")]
    public class Path : ExpressionBase
    {
        public string Expr { get; set; }

        protected override object OnEvaluate(Engine engine)
        {
            if (Expr == null) engine.Throw("path expression not specified");
            return engine.GetPath(Expr, CodeTree);
        }
    }
}
