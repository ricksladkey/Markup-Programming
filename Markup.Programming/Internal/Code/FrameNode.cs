using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming.Core
{
    public abstract class FrameNode : StatementNode
    {
        public StatementNode Body { get; set; }
        protected override void OnExecute(Engine engine)
        {
            engine.With(this, e => OnExecuteFrame(engine));
        }
        protected abstract void OnExecuteFrame(Engine engine);

    }
}
