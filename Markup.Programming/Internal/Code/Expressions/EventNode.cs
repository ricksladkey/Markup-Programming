using System.Collections.Generic;

namespace Markup.Programming.Core
{
    public class EventNode : CallNode
    {
        public string EventName { get; set; }
        public ExpressionNode Handler { get; set; }
        protected override object OnGet(Engine engine)
        {
            return engine.FrameFunc(this, Handler.Get);
        }
        protected override void OnSet(Engine engine, object value)
        {
            engine.FrameAction(this, e => Handler.Set(engine, value));
        }
        public override object OnCall(Engine engine, IEnumerable<object> args)
        {
            return engine.FrameFunc(this, e => (Handler as CallNode).Call(engine, args));
        }
    }
}
