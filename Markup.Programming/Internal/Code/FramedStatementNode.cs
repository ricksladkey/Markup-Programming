using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming.Core
{
    public abstract class FramedStatementNode : StatementNode
    {
        public StatementNode Body { get; set; }
        protected override void OnExecute(Engine engine)
        {
            engine.ExecuteFrame(this, OnExecuteFrame);
        }
        protected abstract void OnExecuteFrame(Engine engine);
    }
}
