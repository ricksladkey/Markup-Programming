using System.Windows.Markup;
using Markup.Programming.Core;

namespace Markup.Programming
{
    [ContentProperty("Expression")]
    public class Path : ExpressionBase
    {
        public string Expression { get; set; }

        protected override object OnGet(Engine engine)
        {
            if (Expression == null) engine.Throw("path expression not specified");
            return engine.GetPath(Expression, CodeTree);
        }
    }
}
