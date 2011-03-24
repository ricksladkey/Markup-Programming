using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using Markup.Programming.Core;

namespace Markup.Programming
{
    [ContentProperty("Script")]
    public class ScriptHandler : EventHandler
    {
        public string Script { get; set; }

        protected override void OnEventHandler(Engine engine)
        {
            engine.SetReturnFrame();
            if (Body == null) return;
            engine.ExecuteScript(Script, CodeTree);
            engine.GetAndResetReturnValue();
        }
    }
}
