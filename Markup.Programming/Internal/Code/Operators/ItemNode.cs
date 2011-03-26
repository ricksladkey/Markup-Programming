using System.Collections.Generic;
using System.Linq;

namespace Markup.Programming.Core
{
    public class ItemNode : ExpressionNode
    {
        public ExpressionNode Context { get; set; }
        public IList<ExpressionNode> Arguments { get; set; }
        protected override object OnGet(Engine engine)
        {
            return PathHelper.GetItem(engine, Context.Get(engine),
                Arguments.Select(arg => arg.Get(engine)).ToArray());
        }
        protected override void OnSet(Engine engine, object value)
        {
            PathHelper.SetItem(engine, Context.Get(engine),
                Arguments.Select(arg => arg.Get(engine)).Concat(new object[] { value }).ToArray());
        }
    }
}
