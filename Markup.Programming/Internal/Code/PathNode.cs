using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming.Core
{
    public class PathNode : CallNode
    {
        public ExpressionNode Path { get; set; }
        protected override object OnGet(Engine engine)
        {
            return engine.FrameFunc(this, e => Path.Get(engine));
        }
        protected override void OnSet(Engine engine, object value)
        {
            engine.FrameAction(this, e => Path.Set(engine, value));
        }
        public override object OnCall(Engine engine, IEnumerable<object> args)
        {
            return engine.FrameFunc(this, e => (Path as CallNode).Call(engine, args));
        }
    }
}
